namespace Schedule1ModdingTool.Services.CodeGeneration.Abstractions
{
    /// <summary>
    /// Fluent builder interface for constructing C# source code.
    /// Provides automatic indentation management and clean API for code generation.
    /// </summary>
    public interface ICodeBuilder
    {
        /// <summary>
        /// Appends a line of code with proper indentation.
        /// </summary>
        /// <param name="line">The line to append. If empty, appends a blank line.</param>
        ICodeBuilder AppendLine(string line = "");

        /// <summary>
        /// Appends multiple lines of code with proper indentation.
        /// </summary>
        ICodeBuilder AppendLines(params string[] lines);

        /// <summary>
        /// Appends a single-line comment.
        /// </summary>
        /// <param name="comment">The comment text (without // prefix).</param>
        ICodeBuilder AppendComment(string comment);

        /// <summary>
        /// Appends an XML documentation comment block.
        /// </summary>
        /// <param name="lines">The summary lines.</param>
        ICodeBuilder AppendBlockComment(params string[] lines);

        /// <summary>
        /// Increases the current indentation level.
        /// </summary>
        ICodeBuilder IncreaseIndent();

        /// <summary>
        /// Decreases the current indentation level.
        /// </summary>
        ICodeBuilder DecreaseIndent();

        /// <summary>
        /// Opens a code block with optional header (e.g., "public class Foo").
        /// Appends "{" and increases indentation.
        /// </summary>
        /// <param name="header">Optional header line before the opening brace.</param>
        ICodeBuilder OpenBlock(string? header = null);

        /// <summary>
        /// Closes a code block. Decreases indentation and appends "}".
        /// </summary>
        /// <param name="semicolon">If true, appends "};" instead of "}".</param>
        ICodeBuilder CloseBlock(bool semicolon = false);

        /// <summary>
        /// Builds and returns the final code string.
        /// </summary>
        string Build();

        /// <summary>
        /// Clears the builder state, allowing reuse.
        /// </summary>
        void Clear();
    }
}
