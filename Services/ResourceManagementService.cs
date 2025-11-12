using System.Diagnostics;
using System.IO;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.Services
{
    /// <summary>
    /// Manages resource file operations including adding, removing, and normalizing resources.
    /// </summary>
    public class ResourceManagementService
    {
        /// <summary>
        /// Result of an add resource operation.
        /// </summary>
        public class AddResourceResult
        {
            public bool Success { get; set; }
            public List<ResourceAsset> AddedAssets { get; set; } = new List<ResourceAsset>();
            public List<string> Failures { get; set; } = new List<string>();
            public ResourceAsset? LastAddedAsset => AddedAssets.LastOrDefault();
        }

        /// <summary>
        /// Adds PNG resources to the project via file dialog.
        /// </summary>
        /// <param name="project">The current project.</param>
        /// <param name="projectDir">The project directory.</param>
        /// <returns>Result containing added assets and any failures.</returns>
        public AddResourceResult AddResources(QuestProject project, string projectDir)
        {
            var result = new AddResourceResult();

            Debug.WriteLine("[ResourceManagementService] Creating OpenFileDialog...");
            var dialog = new OpenFileDialog
            {
                Filter = "PNG Images (*.png)|*.png",
                Title = "Select PNG Resource",
                Multiselect = true,
                CheckFileExists = true
            };

            Debug.WriteLine("[ResourceManagementService] Calling dialog.ShowDialog()...");
            bool? dialogResult;
            try
            {
                dialogResult = dialog.ShowDialog();
                Debug.WriteLine($"[ResourceManagementService] dialog.ShowDialog() returned: {dialogResult}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ResourceManagementService] Exception in ShowDialog: {ex.Message}\n{ex.StackTrace}");
                throw new InvalidOperationException($"Failed to open file dialog: {ex.Message}", ex);
            }

            Debug.WriteLine($"[ResourceManagementService] FileNames.Length: {dialog.FileNames.Length}");
            if (dialogResult != true || dialog.FileNames.Length == 0)
            {
                Debug.WriteLine("[ResourceManagementService] No files selected or dialog cancelled");
                result.Success = false;
                return result;
            }

            Debug.WriteLine($"[ResourceManagementService] Processing {dialog.FileNames.Length} file(s)");
            var resourcesDir = Path.Combine(projectDir, "Resources");
            Debug.WriteLine($"[ResourceManagementService] Resources directory: {resourcesDir}");
            Directory.CreateDirectory(resourcesDir);

            foreach (var file in dialog.FileNames)
            {
                try
                {
                    if (!File.Exists(file))
                    {
                        result.Failures.Add(Path.GetFileName(file));
                        continue;
                    }

                    var baseName = AppUtils.MakeSafeFilename(Path.GetFileNameWithoutExtension(file));
                    var uniqueFileName = GenerateUniqueResourceName(resourcesDir, $"{baseName}.png");
                    var destination = Path.Combine(resourcesDir, uniqueFileName);
                    if (!TryCopyResourceFile(file, destination, out var copyError))
                    {
                        result.Failures.Add($"{Path.GetFileName(file)} ({copyError})");
                        continue;
                    }

                    var asset = new ResourceAsset
                    {
                        DisplayName = baseName,
                        RelativePath = Path.Combine("Resources", uniqueFileName).Replace('\\', '/')
                    };

                    project.AddResource(asset);
                    result.AddedAssets.Add(asset);
                }
                catch (Exception ex)
                {
                    result.Failures.Add($"{Path.GetFileName(file)} ({ex.Message})");
                }
            }

            Debug.WriteLine($"[ResourceManagementService] Successfully added {result.AddedAssets.Count} asset(s), {result.Failures.Count} failure(s)");
            result.Success = result.AddedAssets.Count > 0;
            return result;
        }

        /// <summary>
        /// Removes a resource from the project and deletes the file.
        /// </summary>
        /// <param name="project">The current project.</param>
        /// <param name="resource">The resource to remove.</param>
        /// <param name="projectDir">The project directory.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool RemoveResource(QuestProject project, ResourceAsset resource, string projectDir)
        {
            if (resource == null || project == null)
                return false;

            var absolutePath = Path.Combine(projectDir,
                (resource.RelativePath ?? string.Empty).Replace('/', Path.DirectorySeparatorChar));

            try
            {
                if (File.Exists(absolutePath))
                {
                    if (!TryDeleteFileWithRetry(absolutePath, out var delError))
                    {
                        AppUtils.ShowWarning($"Failed to delete file '{absolutePath}': {delError}");
                        // Continue to remove from project even if file deletion fails
                    }
                }
            }
            catch (Exception ex)
            {
                AppUtils.ShowWarning($"Failed to delete file '{absolutePath}': {ex.Message}");
                // Continue to remove from project even if file deletion fails
            }

            project.RemoveResource(resource);
            return true;
        }

        /// <summary>
        /// Normalizes all project resources, copying external files into the project Resources folder.
        /// </summary>
        /// <param name="project">The project to normalize.</param>
        /// <param name="projectDir">The project directory.</param>
        public void NormalizeProjectResources(QuestProject project, string projectDir)
        {
            try
            {
                if (project == null || project.Resources.Count == 0)
                {
                    Debug.WriteLine("[NormalizeProjectResources] No project/resources to normalize");
                    return;
                }

                var resourcesDir = Path.Combine(projectDir, "Resources");
                Directory.CreateDirectory(resourcesDir);
                var resourcesDirFull = NormalizeDirectoryPath(resourcesDir);

                Debug.WriteLine($"[NormalizeProjectResources] Normalizing {project.Resources.Count} resource(s) into '{resourcesDirFull}'");
                foreach (var asset in project.Resources.ToList())
                {
                    EnsureResourceAssetLocal(asset, projectDir, resourcesDir, resourcesDirFull);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NormalizeProjectResources] {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Validates all resources in the project and returns a list of missing resources.
        /// </summary>
        /// <param name="project">The project to validate.</param>
        /// <param name="projectDir">The project directory.</param>
        /// <returns>List of missing resource display names and paths, or empty list if all valid.</returns>
        public List<string> ValidateProjectResources(QuestProject project, string projectDir)
        {
            var missingResources = new List<string>();

            if (project == null || project.Resources.Count == 0)
                return missingResources;

            foreach (var asset in project.Resources)
            {
                if (string.IsNullOrWhiteSpace(asset.RelativePath))
                {
                    missingResources.Add($"{asset.DisplayName}: No path specified");
                    continue;
                }

                if (!ResourcePathHelper.ResourceExists(asset.RelativePath, projectDir))
                {
                    var expectedPath = Path.Combine(projectDir, asset.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                    missingResources.Add($"{asset.DisplayName} ({asset.RelativePath})\n  Expected at: {expectedPath}");
                }
            }

            return missingResources;
        }

        private void EnsureResourceAssetLocal(ResourceAsset asset, string projectDir, string resourcesDir, string resourcesDirFull)
        {
            if (asset == null)
            {
                Debug.WriteLine("[EnsureResourceAssetLocal] Asset was null");
                return;
            }

            var relativePath = asset.RelativePath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                Debug.WriteLine("[EnsureResourceAssetLocal] Asset relative path was null/empty");
                return;
            }

            string absolutePath;
            try
            {
                absolutePath = ResourcePathHelper.GetAbsolutePath(relativePath, projectDir);
            }
            catch
            {
                absolutePath = relativePath;
            }

            if (!File.Exists(absolutePath) && Path.IsPathRooted(relativePath) && File.Exists(relativePath))
            {
                absolutePath = relativePath;
            }

            if (!File.Exists(absolutePath))
            {
                Debug.WriteLine($"[EnsureResourceAssetLocal] File missing for '{relativePath}' (expected '{absolutePath}')");
                return;
            }

            Debug.WriteLine($"[EnsureResourceAssetLocal] Found '{absolutePath}' for '{relativePath}'");
            var absoluteFull = NormalizeDirectoryPath(absolutePath);
            if (!absoluteFull.StartsWith(resourcesDirFull, StringComparison.OrdinalIgnoreCase))
            {
                var extension = Path.GetExtension(absolutePath);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".png";
                }

                var safeName = AppUtils.MakeSafeFilename(Path.GetFileNameWithoutExtension(absolutePath));
                var destinationName = GenerateUniqueResourceName(resourcesDir, $"{safeName}{extension}");
                var destinationPath = Path.Combine(resourcesDir, destinationName);
                Debug.WriteLine($"[EnsureResourceAssetLocal] Copying '{absoluteFull}' into project resources '{destinationPath}'");

                if (!TryCopyResourceFile(absolutePath, destinationPath, out var error))
                {
                    Debug.WriteLine($"[EnsureResourceAssetLocal] Failed to copy {absolutePath}: {error}");
                    return;
                }

                absoluteFull = NormalizeDirectoryPath(destinationPath);
                absolutePath = destinationPath;
            }

            var normalizedRelative = ResourcePathHelper.GetProjectRelativePath(absolutePath, projectDir);
            if (!string.Equals(asset.RelativePath, normalizedRelative, StringComparison.OrdinalIgnoreCase))
            {
                asset.RelativePath = normalizedRelative;
            }
        }

        private static string GenerateUniqueResourceName(string directory, string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var candidate = fileName;
            var counter = 1;

            while (File.Exists(Path.Combine(directory, candidate)))
            {
                candidate = $"{name}_{counter++}{ext}";
            }

            return candidate;
        }

        private static string NormalizeDirectoryPath(string path)
        {
            return Path.GetFullPath(path)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool TryCopyResourceFile(string source, string destination, out string error)
        {
            error = string.Empty;

            // Retry copy to mitigate transient file locks from antivirus/indexers or image decoders
            const int maxRetries = 5;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Debug.WriteLine($"[TryCopyResourceFile] Attempt {attempt}/{maxRetries}: '{source}' -> '{destination}'");
                    var destDir = Path.GetDirectoryName(destination);
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    if (File.Exists(destination))
                    {
                        try
                        {
                            File.Delete(destination);
                        }
                        catch (IOException ioEx)
                        {
                            Debug.WriteLine($"[TryCopyResourceFile] Destination delete failed: {ioEx.Message}");
                            // Destination locked; wait and retry delete on next attempt
                        }
                    }

                    // Use FileStream with FileShare.ReadWrite to allow reading even if file is open elsewhere
                    // This allows copying files that might be open in Explorer preview or image viewers
                    using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (var destStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        sourceStream.CopyTo(destStream);
                    }
                    Debug.WriteLine($"[TryCopyResourceFile] Copy succeeded: '{destination}'");
                    return true;
                }
                catch (IOException ioEx)
                {
                    error = ioEx.Message;
                    Debug.WriteLine($"[TryCopyResourceFile] IO exception ({attempt}/{maxRetries}): {ioEx.Message}");
                    if (attempt == maxRetries)
                        break;

                    // Exponential backoff: 100ms, 200ms, 400ms, 800ms, 1600ms
                    System.Threading.Thread.Sleep(100 * (int)Math.Pow(2, attempt - 1));
                    continue;
                }
                catch (UnauthorizedAccessException unauthEx)
                {
                    error = unauthEx.Message;
                    Debug.WriteLine($"[TryCopyResourceFile] Unauthorized ({attempt}/{maxRetries}): {unauthEx.Message}");
                    if (attempt == maxRetries)
                        break;
                    System.Threading.Thread.Sleep(100 * (int)Math.Pow(2, attempt - 1));
                    continue;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    Debug.WriteLine($"[TryCopyResourceFile] Fatal error: {ex.Message}");
                    return false;
                }
            }

            Debug.WriteLine($"[TryCopyResourceFile] Failed after {maxRetries} attempts: '{source}' -> '{destination}' ({error})");
            return false;
        }

        private static bool TryDeleteFileWithRetry(string absolutePath, out string error)
        {
            error = string.Empty;
            const int maxRetries = 5;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (File.Exists(absolutePath))
                    {
                        File.Delete(absolutePath);
                    }
                    return true;
                }
                catch (IOException ioEx)
                {
                    error = ioEx.Message;
                    if (attempt == maxRetries)
                        break;
                    System.Threading.Thread.Sleep(100 * (int)Math.Pow(2, attempt - 1));
                }
                catch (UnauthorizedAccessException unauthEx)
                {
                    error = unauthEx.Message;
                    if (attempt == maxRetries)
                        break;
                    System.Threading.Thread.Sleep(100 * (int)Math.Pow(2, attempt - 1));
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }
            return false;
        }
    }
}
