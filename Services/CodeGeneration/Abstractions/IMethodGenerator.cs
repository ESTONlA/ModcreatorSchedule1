using System;

namespace Schedule1ModdingTool.Services.CodeGeneration.Abstractions
{
    /// <summary>
    /// Generates code for a specific method.
    /// Implementations handle method signature and body generation.
    /// </summary>
    public interface IMethodGenerator
    {
        /// <summary>
        /// Generates the complete method code including signature and body.
        /// </summary>
        /// <param name="builder">The code builder to append to.</param>
        void Generate(ICodeBuilder builder);
    }
}
