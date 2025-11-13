using System.Reflection;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Provides version information for the application
    /// </summary>
    public static class VersionInfo
    {
        private static readonly string? _version;

        static VersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            
            if (versionAttribute != null && !string.IsNullOrEmpty(versionAttribute.InformationalVersion))
            {
                _version = versionAttribute.InformationalVersion;
            }
            else
            {
                var version = assembly.GetName().Version;
                _version = version != null ? version.ToString() : "1.0.0";
            }
        }

        /// <summary>
        /// Gets the full version string (e.g., "1.0.0-beta.1" or "1.0.0")
        /// </summary>
        public static string Version => _version ?? "1.0.0";

        /// <summary>
        /// Gets whether this is a beta/pre-release version
        /// </summary>
        public static bool IsBeta => Version.Contains("-beta", StringComparison.OrdinalIgnoreCase) ||
                                     Version.Contains("-alpha", StringComparison.OrdinalIgnoreCase) ||
                                     Version.Contains("-pre", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the base version number without pre-release suffix (e.g., "1.0.0" from "1.0.0-beta.1")
        /// </summary>
        public static string BaseVersion
        {
            get
            {
                var version = Version;
                var dashIndex = version.IndexOf('-');
                return dashIndex >= 0 ? version.Substring(0, dashIndex) : version;
            }
        }
    }
}

