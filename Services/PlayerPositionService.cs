using System.IO.Pipes;
using System.Text;
using Newtonsoft.Json;

namespace Schedule1ModdingTool.Services
{
    /// <summary>
    /// Service that requests player position from the Connector mod via named pipe.
    /// </summary>
    public class PlayerPositionService : IDisposable
    {
        private const string PipeName = "Schedule1ModCreator_Position";
        private const int RequestTimeoutMs = 3000;

        /// <summary>
        /// Represents a position response from the connector mod.
        /// </summary>
        public class PositionResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("position")]
            public PositionData? Position { get; set; }

            [JsonProperty("error")]
            public string? Error { get; set; }
        }

        /// <summary>
        /// Represents position data (x, y, z).
        /// </summary>
        public class PositionData
        {
            [JsonProperty("x")]
            public float X { get; set; }

            [JsonProperty("y")]
            public float Y { get; set; }

            [JsonProperty("z")]
            public float Z { get; set; }
        }

        /// <summary>
        /// Requests the current player position from the connector mod.
        /// </summary>
        /// <returns>Position response, or null if request failed or timed out.</returns>
        public PositionResponse? RequestPlayerPosition()
        {
            NamedPipeClientStream? pipeClient = null;
            try
            {
                pipeClient = new NamedPipeClientStream(
                    ".",
                    PipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous);

                // Try to connect with timeout
                if (!pipeClient.IsConnected)
                {
                    var connectTask = pipeClient.ConnectAsync(RequestTimeoutMs);
                    connectTask.Wait(RequestTimeoutMs);
                }

                if (!pipeClient.IsConnected)
                {
                    return new PositionResponse
                    {
                        Success = false,
                        Error = "Failed to connect to connector mod"
                    };
                }

                // Send request
                var request = new { request = "getPosition" };
                var requestJson = JsonConvert.SerializeObject(request);
                var requestBytes = Encoding.UTF8.GetBytes(requestJson);
                var lengthBytes = BitConverter.GetBytes(requestBytes.Length);

                pipeClient.Write(lengthBytes, 0, lengthBytes.Length);
                pipeClient.Write(requestBytes, 0, requestBytes.Length);
                pipeClient.Flush();

                // Read response length
                var responseLengthBytes = new byte[4];
                var bytesRead = pipeClient.Read(responseLengthBytes, 0, 4);
                if (bytesRead != 4)
                {
                    return new PositionResponse
                    {
                        Success = false,
                        Error = "Failed to read response length"
                    };
                }

                var responseLength = BitConverter.ToInt32(responseLengthBytes, 0);
                if (responseLength <= 0 || responseLength > 1024 * 1024) // Max 1MB
                {
                    return new PositionResponse
                    {
                        Success = false,
                        Error = "Invalid response length"
                    };
                }

                // Read response data
                var responseBytes = new byte[responseLength];
                var totalRead = 0;
                while (totalRead < responseLength)
                {
                    var read = pipeClient.Read(responseBytes, totalRead, responseLength - totalRead);
                    if (read == 0)
                    {
                        return new PositionResponse
                        {
                            Success = false,
                            Error = "Connection closed while reading response"
                        };
                    }
                    totalRead += read;
                }

                var responseJson = Encoding.UTF8.GetString(responseBytes);
                var response = JsonConvert.DeserializeObject<PositionResponse>(responseJson);
                return response;
            }
            catch (TimeoutException)
            {
                return new PositionResponse
                {
                    Success = false,
                    Error = "Request timed out"
                };
            }
            catch (Exception ex)
            {
                return new PositionResponse
                {
                    Success = false,
                    Error = $"Error requesting position: {ex.Message}"
                };
            }
            finally
            {
                try
                {
                    pipeClient?.Dispose();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            // Nothing to dispose for this service
        }
    }
}

