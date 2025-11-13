using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Schedule1ModdingTool.Models;

namespace Schedule1ModdingTool.Services
{
    /// <summary>
    /// Service that receives exceptions from generated mods via named pipe and raises events for UI handling.
    /// </summary>
    public class ExceptionReceiverService : IDisposable
    {
        private const string PipeName = "Schedule1ModCreator_Exceptions";
        private const int ConnectionRetryDelayMs = 2000;

        private NamedPipeClientStream? _pipeClient;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveTask;
        private bool _isDisposed;

        /// <summary>
        /// Event raised when an exception is received from a generated mod.
        /// </summary>
        public event EventHandler<ModExceptionEventArgs>? ExceptionReceived;

        /// <summary>
        /// Starts receiving exceptions from the connector mod.
        /// </summary>
        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ExceptionReceiverService));

            if (_receiveTask != null && !_receiveTask.IsCompleted)
                return; // Already started

            _cancellationTokenSource = new CancellationTokenSource();
            _receiveTask = Task.Run(async () => await ReceiveLoop(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Stops receiving exceptions.
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource?.Cancel();

            try
            {
                _receiveTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
                // Task may have been cancelled, ignore
            }
            catch (Exception)
            {
                // Ignore other exceptions during shutdown
            }

            _receiveTask = null;

            try
            {
                _pipeClient?.Dispose();
            }
            catch { }
            _pipeClient = null;
        }

        /// <summary>
        /// Main receive loop that connects to the connector mod and receives exceptions.
        /// </summary>
        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Try to connect
                    _pipeClient = new NamedPipeClientStream(
                        ".",
                        PipeName,
                        PipeDirection.In,
                        PipeOptions.Asynchronous);

                    await _pipeClient.ConnectAsync(5000, cancellationToken);

                    if (_pipeClient.IsConnected)
                    {
                        // Connected - start receiving exceptions
                        while (_pipeClient.IsConnected && !cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                var exceptionData = ReceiveException();
                                if (exceptionData != null)
                                {
                                    OnExceptionReceived(exceptionData);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log error but continue receiving
                                System.Diagnostics.Debug.WriteLine($"ExceptionReceiverService: Error receiving exception: {ex.Message}");
                                break; // Break to reconnect
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (TimeoutException)
                {
                    // Connection timeout - wait and retry (this is expected when connector mod isn't running)
                    System.Diagnostics.Debug.WriteLine("ExceptionReceiverService: Connection timeout - connector mod may not be running");
                }
                catch (Exception ex)
                {
                    // Connection failed or lost - wait and retry
                    System.Diagnostics.Debug.WriteLine($"ExceptionReceiverService: Connection error: {ex.Message}");
                }
                finally
                {
                    try
                    {
                        _pipeClient?.Dispose();
                    }
                    catch { }
                    _pipeClient = null;
                }

                // Wait before retrying connection
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(ConnectionRetryDelayMs, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Receives a single exception from the pipe.
        /// </summary>
        private ModExceptionData? ReceiveException()
        {
            if (_pipeClient == null || !_pipeClient.IsConnected)
                return null;

            try
            {
                // Read length
                var lengthBytes = new byte[4];
                var bytesRead = _pipeClient.Read(lengthBytes, 0, 4);
                if (bytesRead != 4)
                {
                    return null; // Connection closed or invalid data
                }

                var length = BitConverter.ToInt32(lengthBytes, 0);
                if (length <= 0 || length > 1024 * 1024) // Max 1MB
                {
                    return null; // Invalid length
                }

                // Read data
                var dataBytes = new byte[length];
                var totalRead = 0;
                while (totalRead < length)
                {
                    if (!_pipeClient.IsConnected)
                    {
                        return null;
                    }
                    var read = _pipeClient.Read(dataBytes, totalRead, length - totalRead);
                    if (read == 0)
                    {
                        return null;
                    }
                    totalRead += read;
                }

                // Deserialize
                var json = Encoding.UTF8.GetString(dataBytes);
                var exceptionData = JsonConvert.DeserializeObject<ModExceptionData>(json);
                return exceptionData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExceptionReceiverService: Error reading exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Raises the ExceptionReceived event.
        /// </summary>
        private void OnExceptionReceived(ModExceptionData exceptionData)
        {
            ExceptionReceived?.Invoke(this, new ModExceptionEventArgs(exceptionData));
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            Stop();

            try
            {
                _receiveTask?.Wait(TimeSpan.FromSeconds(1));
            }
            catch { }

            _cancellationTokenSource?.Dispose();
            _receiveTask = null;
        }
    }

    /// <summary>
    /// Event arguments for exception received events.
    /// </summary>
    public class ModExceptionEventArgs : EventArgs
    {
        public ModExceptionData ExceptionData { get; }

        public ModExceptionEventArgs(ModExceptionData exceptionData)
        {
            ExceptionData = exceptionData ?? throw new ArgumentNullException(nameof(exceptionData));
        }
    }
}

