using Newtonsoft.Json;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents a resource asset (e.g., PNG) stored with the project.
    /// </summary>
    public class ResourceAsset : ObservableObject
    {
        private string _displayName = "Resource";
        private string _relativePath = string.Empty;

        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [JsonProperty("displayName")]
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Relative path (from project root) to the asset on disk, e.g. "Resources/my-icon.png".
        /// </summary>
        [JsonProperty("relativePath")]
        public string RelativePath
        {
            get => _relativePath;
            set => SetProperty(ref _relativePath, value);
        }

        /// <summary>
        /// Optional description or tag.
        /// </summary>
        [JsonProperty("notes")]
        public string? Notes { get; set; }
    }
}
