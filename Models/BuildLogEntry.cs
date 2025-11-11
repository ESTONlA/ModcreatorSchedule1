using System;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents a single build log entry (error or warning)
    /// </summary>
    public class BuildLogEntry
    {
        public string Message { get; set; } = "";
        public string? FilePath { get; set; }
        public int? LineNumber { get; set; }
        public int? ColumnNumber { get; set; }
        public string? ErrorCode { get; set; }
        public string FullText { get; set; } = "";
        public bool IsError { get; set; }
    }
}

