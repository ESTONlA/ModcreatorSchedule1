using System;
using System.Linq;
using System.Text;
using Schedule1ModdingTool.Services.CodeGeneration.Abstractions;

namespace Schedule1ModdingTool.Services.CodeGeneration.Builders
{
    /// <summary>
    /// Fluent builder for constructing properly indented C# source code.
    /// Manages indentation automatically and provides a clean API for code generation.
    /// </summary>
    public class CodeBuilder : ICodeBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indentLevel = 0;
        private const string IndentString = "    "; // 4 spaces

        /// <summary>
        /// Appends a line of code with proper indentation.
        /// </summary>
        /// <param name="line">The line to append. If empty, appends a blank line.</param>
        public ICodeBuilder AppendLine(string line = "")
        {
            if (string.IsNullOrEmpty(line))
            {
                _sb.AppendLine();
            }
            else
            {
                var indent = string.Concat(Enumerable.Repeat(IndentString, _indentLevel));
                _sb.AppendLine(indent + line);
            }
            return this;
        }

        /// <summary>
        /// Appends multiple lines of code with proper indentation.
        /// </summary>
        public ICodeBuilder AppendLines(params string[] lines)
        {
            foreach (var line in lines)
            {
                AppendLine(line);
            }
            return this;
        }

        /// <summary>
        /// Appends a single-line comment.
        /// </summary>
        /// <param name="comment">The comment text (without // prefix).</param>
        public ICodeBuilder AppendComment(string comment)
        {
            return AppendLine($"// {comment}");
        }

        /// <summary>
        /// Appends an XML documentation comment block.
        /// </summary>
        /// <param name="lines">The summary lines.</param>
        public ICodeBuilder AppendBlockComment(params string[] lines)
        {
            AppendLine("/// <summary>");
            foreach (var line in lines)
            {
                AppendLine($"/// {line}");
            }
            AppendLine("/// </summary>");
            return this;
        }

        /// <summary>
        /// Increases the current indentation level.
        /// </summary>
        public ICodeBuilder IncreaseIndent()
        {
            _indentLevel++;
            return this;
        }

        /// <summary>
        /// Decreases the current indentation level.
        /// </summary>
        public ICodeBuilder DecreaseIndent()
        {
            if (_indentLevel > 0)
            {
                _indentLevel--;
            }
            return this;
        }

        /// <summary>
        /// Opens a code block with optional header (e.g., "public class Foo").
        /// Appends "{" and increases indentation.
        /// </summary>
        /// <param name="header">Optional header line before the opening brace.</param>
        public ICodeBuilder OpenBlock(string? header = null)
        {
            if (header != null)
            {
                AppendLine(header);
            }
            AppendLine("{");
            IncreaseIndent();
            return this;
        }

        /// <summary>
        /// Closes a code block. Decreases indentation and appends "}".
        /// </summary>
        /// <param name="semicolon">If true, appends "};" instead of "}".</param>
        public ICodeBuilder CloseBlock(bool semicolon = false)
        {
            DecreaseIndent();
            AppendLine(semicolon ? "};" : "}");
            return this;
        }

        /// <summary>
        /// Builds and returns the final code string.
        /// </summary>
        public string Build()
        {
            return _sb.ToString();
        }

        /// <summary>
        /// Clears the builder state, allowing reuse.
        /// </summary>
        public void Clear()
        {
            _sb.Clear();
            _indentLevel = 0;
        }
    }
}
