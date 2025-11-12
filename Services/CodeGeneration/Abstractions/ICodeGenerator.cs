namespace Schedule1ModdingTool.Services.CodeGeneration.Abstractions
{
    /// <summary>
    /// Defines a contract for generating C# source code from a blueprint.
    /// </summary>
    /// <typeparam name="TBlueprint">The type of blueprint to generate code from.</typeparam>
    public interface ICodeGenerator<in TBlueprint>
    {
        /// <summary>
        /// Generates complete C# source code from the given blueprint.
        /// </summary>
        /// <param name="blueprint">The blueprint to generate code from.</param>
        /// <returns>Complete C# source code as a string.</returns>
        string GenerateCode(TBlueprint blueprint);

        /// <summary>
        /// Validates whether the blueprint can be successfully generated.
        /// </summary>
        /// <param name="blueprint">The blueprint to validate.</param>
        /// <returns>Validation result with any errors or warnings.</returns>
        CodeGenerationValidationResult Validate(TBlueprint blueprint);
    }

    /// <summary>
    /// Result of validating a blueprint before code generation.
    /// </summary>
    public class CodeGenerationValidationResult
    {
        /// <summary>
        /// Whether the blueprint is valid for generation.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Critical errors that prevent generation.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Non-critical warnings about the blueprint.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
