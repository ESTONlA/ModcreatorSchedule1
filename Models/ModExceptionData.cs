using System;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents exception data received from a generated mod.
    /// </summary>
    public class ModExceptionData
    {
        public string ExceptionType { get; set; } = "";
        public string Message { get; set; } = "";
        public string? StackTrace { get; set; }
        public string? SourceAssembly { get; set; }
        public string? ModName { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsUnhandled { get; set; }
        public ModExceptionData? InnerException { get; set; }

        /// <summary>
        /// Gets a formatted string representation of the exception.
        /// </summary>
        public string GetFormattedString()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"{(IsUnhandled ? "UNHANDLED" : "First-Chance")} Exception");
            if (!string.IsNullOrEmpty(ModName))
            {
                sb.AppendLine($"Mod: {ModName}");
            }
            if (!string.IsNullOrEmpty(SourceAssembly))
            {
                sb.AppendLine($"Assembly: {SourceAssembly}");
            }
            sb.AppendLine($"Type: {ExceptionType}");
            sb.AppendLine($"Time: {Timestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine($"Message: {Message}");
            
            if (!string.IsNullOrEmpty(StackTrace))
            {
                sb.AppendLine();
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(StackTrace);
            }

            if (InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(InnerException.GetFormattedString());
            }

            return sb.ToString();
        }
    }
}

