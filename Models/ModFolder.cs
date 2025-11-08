using System;
using Newtonsoft.Json;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents a user-defined folder in the workspace explorer.
    /// </summary>
    public class ModFolder : ObservableObject
    {
        private string _name = "New Folder";
        private string? _parentId;

        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, string.IsNullOrWhiteSpace(value) ? "New Folder" : value);
        }

        [JsonProperty("parentId")]
        public string? ParentId
        {
            get => _parentId;
            set => SetProperty(ref _parentId, value);
        }
    }
}
