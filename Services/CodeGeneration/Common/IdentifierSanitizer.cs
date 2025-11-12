using System.Text;

namespace Schedule1ModdingTool.Services.CodeGeneration.Common
{
    /// <summary>
    /// Sanitizes and validates C# identifiers.
    /// Ensures generated identifiers comply with C# naming rules.
    /// </summary>
    public static class IdentifierSanitizer
    {
        /// <summary>
        /// Converts a candidate string into a valid C# identifier.
        /// Replaces invalid characters with underscores and ensures the identifier starts with a letter or underscore.
        /// </summary>
        /// <param name="candidate">The string to sanitize.</param>
        /// <param name="fallback">Fallback identifier if candidate is invalid or empty.</param>
        /// <returns>A valid C# identifier.</returns>
        public static string MakeSafeIdentifier(string? candidate, string fallback)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return fallback;
            }

            var builder = new StringBuilder();
            foreach (var ch in candidate)
            {
                if (builder.Length == 0)
                {
                    // First character: must be letter or underscore
                    if (char.IsLetter(ch) || ch == '_')
                    {
                        builder.Append(ch);
                    }
                    else if (char.IsDigit(ch))
                    {
                        // Prefix with underscore if starts with digit
                        builder.Append('_').Append(ch);
                    }
                    // Skip other invalid characters at the start
                }
                else
                {
                    // Subsequent characters: letter, digit, or underscore
                    if (char.IsLetterOrDigit(ch) || ch == '_')
                    {
                        builder.Append(ch);
                    }
                    else
                    {
                        // Replace invalid characters with underscore
                        builder.Append('_');
                    }
                }
            }

            var result = builder.ToString();
            return string.IsNullOrEmpty(result) ? fallback : result;
        }

        /// <summary>
        /// Ensures an identifier is unique within a set of used names.
        /// Appends numeric suffixes (_1, _2, etc.) until a unique name is found.
        /// </summary>
        /// <param name="identifier">The desired identifier.</param>
        /// <param name="usedNames">Set of already-used names (case-insensitive comparison).</param>
        /// <param name="fallbackIndex">Index to use for fallback naming if identifier is empty.</param>
        /// <returns>A unique identifier that has been added to the usedNames set.</returns>
        public static string EnsureUniqueIdentifier(
            string identifier,
            HashSet<string> usedNames,
            int fallbackIndex)
        {
            if (usedNames == null)
                throw new ArgumentNullException(nameof(usedNames));

            var baseName = string.IsNullOrWhiteSpace(identifier)
                ? $"item{fallbackIndex}"
                : identifier;

            var uniqueName = baseName;
            var suffix = 1;

            // Keep appending suffixes until we find a unique name
            while (!usedNames.Add(uniqueName))
            {
                uniqueName = $"{baseName}_{suffix++}";
            }

            return uniqueName;
        }

        /// <summary>
        /// Checks if a string is a valid C# identifier according to language rules.
        /// </summary>
        /// <param name="identifier">The identifier to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            // Check first character: must be letter or underscore
            if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
                return false;

            // Check remaining characters: must be letter, digit, or underscore
            return identifier.Skip(1).All(ch => char.IsLetterOrDigit(ch) || ch == '_');
        }

        /// <summary>
        /// Checks if a string is a C# reserved keyword.
        /// Reserved keywords cannot be used as identifiers without the @ prefix.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>True if the identifier is a reserved keyword.</returns>
        public static bool IsReservedKeyword(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            // C# reserved keywords
            var keywords = new HashSet<string>(StringComparer.Ordinal)
            {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
                "checked", "class", "const", "continue", "decimal", "default", "delegate",
                "do", "double", "else", "enum", "event", "explicit", "extern", "false",
                "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
                "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
                "new", "null", "object", "operator", "out", "override", "params", "private",
                "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
                "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
                "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
                "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
            };

            return keywords.Contains(identifier);
        }

        /// <summary>
        /// Makes a safe identifier and escapes it with @ if it's a reserved keyword.
        /// </summary>
        /// <param name="candidate">The string to sanitize.</param>
        /// <param name="fallback">Fallback identifier if candidate is invalid.</param>
        /// <returns>A valid, non-reserved C# identifier.</returns>
        public static string MakeSafeNonReservedIdentifier(string? candidate, string fallback)
        {
            var safe = MakeSafeIdentifier(candidate, fallback);

            if (IsReservedKeyword(safe))
            {
                // Append underscore to avoid keyword collision
                return safe + "_";
            }

            return safe;
        }
    }
}
