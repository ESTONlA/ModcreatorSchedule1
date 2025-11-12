namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents a contributor to the project with donation information.
    /// </summary>
    public class Contributor
    {
        /// <summary>
        /// Gets or sets the contributor's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relative path to the contributor's profile picture in Resources.
        /// </summary>
        public string ProfilePicturePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Ko-fi donation URL for this contributor.
        /// </summary>
        public string KoFiUrl { get; set; } = string.Empty;
    }
}

