using System;
using System.IO;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Helpers for resolving resource asset paths relative to a project file.
    /// </summary>
    public static class ResourcePathHelper
    {
        public static string GetAbsolutePath(string? resourcePath, string projectDirectory)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                return string.Empty;

            if (Path.IsPathRooted(resourcePath))
            {
                return Path.GetFullPath(resourcePath);
            }

            var normalized = resourcePath.Replace('/', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(projectDirectory, normalized));
        }

        public static string GetProjectRelativePath(string absolutePath, string projectDirectory)
        {
            var fullPath = Path.GetFullPath(absolutePath);
            var fullProject = Path.GetFullPath(projectDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (!fullPath.StartsWith(fullProject, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Replace(Path.DirectorySeparatorChar, '/');
            }

            var relative = fullPath.Substring(fullProject.Length)
                                   .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        }

        public static bool IsInsideProject(string absolutePath, string projectDirectory)
        {
            var fullPath = Path.GetFullPath(absolutePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fullProject = Path.GetFullPath(projectDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return fullPath.StartsWith(fullProject, StringComparison.OrdinalIgnoreCase);
        }
    }
}
