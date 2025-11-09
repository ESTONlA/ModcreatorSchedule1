using System;
using System.Globalization;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.Services.CodeGeneration.Common
{
    /// <summary>
    /// Provides formatting utilities for C# code generation.
    /// Handles string escaping, numeric formatting, and Unity type expression generation.
    /// </summary>
    public static class CodeFormatter
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Escapes a string for use in C# string literals.
        /// Handles backslashes, quotes, and newline characters.
        /// </summary>
        /// <param name="input">The string to escape.</param>
        /// <returns>An escaped string safe for C# code generation.</returns>
        public static string EscapeString(string? input)
        {
            return input?
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") ?? string.Empty;
        }

        /// <summary>
        /// Formats a double value for C# code with up to 3 decimal places.
        /// Uses invariant culture to ensure consistent output across locales.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted string (e.g., "1.5", "2.125", "3").</returns>
        public static string FormatFloat(double value)
        {
            return value.ToString("0.###", InvariantCulture);
        }

        /// <summary>
        /// Generates a Vector3 constructor expression for Unity code.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <returns>C# expression like "new Vector3(1.5f, 2f, 3.125f)".</returns>
        public static string FormatVector3(double x, double y, double z)
        {
            var fx = FormatFloat(x);
            var fy = FormatFloat(y);
            var fz = FormatFloat(z);
            return $"new Vector3({fx}f, {fy}f, {fz}f)";
        }

        /// <summary>
        /// Generates a Vector3 expression from a quest objective's location.
        /// Returns "Vector3.zero" if the objective has no location.
        /// </summary>
        /// <param name="objective">The quest objective.</param>
        /// <returns>Vector3 expression or "Vector3.zero".</returns>
        public static string FormatVector3(QuestObjective objective)
        {
            if (!objective.HasLocation)
            {
                return "Vector3.zero";
            }

            return FormatVector3(objective.LocationX, objective.LocationY, objective.LocationZ);
        }

        /// <summary>
        /// Generates a Color32 constructor expression for Unity code.
        /// </summary>
        /// <param name="r">Red component (0-255).</param>
        /// <param name="g">Green component (0-255).</param>
        /// <param name="b">Blue component (0-255).</param>
        /// <param name="a">Alpha component (0-255), defaults to 255.</param>
        /// <returns>C# expression like "new Color32(255, 128, 64, 255)".</returns>
        public static string FormatColor32(byte r, byte g, byte b, byte a = 255)
        {
            return $"new Color32({r}, {g}, {b}, {a})";
        }

        /// <summary>
        /// Generates a Color constructor expression for Unity code.
        /// </summary>
        /// <param name="r">Red component (0-1).</param>
        /// <param name="g">Green component (0-1).</param>
        /// <param name="b">Blue component (0-1).</param>
        /// <param name="a">Alpha component (0-1), defaults to 1.</param>
        /// <returns>C# expression like "new Color(1f, 0.5f, 0.25f, 1f)".</returns>
        public static string FormatColor(float r, float g, float b, float a = 1f)
        {
            return $"new Color({FormatFloat(r)}f, {FormatFloat(g)}f, {FormatFloat(b)}f, {FormatFloat(a)}f)";
        }

        /// <summary>
        /// Generates a Color expression from a hex color string using ColorUtils.
        /// Delegates to the existing ColorUtils.ToUnityColorExpression method.
        /// </summary>
        /// <param name="hexColor">Hex color string (e.g., "#FF0000").</param>
        /// <returns>Unity Color constructor expression.</returns>
        public static string FormatColorFromHex(string? hexColor)
        {
            return ColorUtils.ToUnityColorExpression(hexColor);
        }

        /// <summary>
        /// Generates a Color32 expression from a hex color string using ColorUtils.
        /// Delegates to the existing ColorUtils.ToUnityColor32Expression method.
        /// </summary>
        /// <param name="hexColor">Hex color string (e.g., "#FF0000").</param>
        /// <returns>Unity Color32 constructor expression.</returns>
        public static string FormatColor32FromHex(string? hexColor)
        {
            return ColorUtils.ToUnityColor32Expression(hexColor);
        }

        /// <summary>
        /// Generates a tuple expression for two float values.
        /// Delegates to the existing ColorUtils.ToTupleExpression method.
        /// </summary>
        /// <param name="left">First value.</param>
        /// <param name="right">Second value.</param>
        /// <returns>Tuple expression like "(0.5f, 0.75f)".</returns>
        public static string FormatTuple(float left, float right)
        {
            return ColorUtils.ToTupleExpression(left, right);
        }
    }
}
