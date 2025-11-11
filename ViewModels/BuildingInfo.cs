namespace Schedule1ModdingTool.ViewModels
{
    /// <summary>
    /// Information about a building identifier type for use in building selectors.
    /// </summary>
    public class BuildingInfo
    {
        public string TypeName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string FullTypeName { get; set; } = "";

        public override string ToString()
        {
            return $"{DisplayName} ({TypeName})";
        }
    }
}

