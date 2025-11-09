using System;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services.CodeGeneration.Abstractions;

namespace Schedule1ModdingTool.Services.CodeGeneration.Quest
{
    /// <summary>
    /// Generates file header comments for quest blueprints.
    /// Creates a standardized header with mod and game information.
    /// </summary>
    public class QuestHeaderGenerator
    {
        /// <summary>
        /// Generates the header comment block for a quest file.
        /// </summary>
        /// <param name="builder">The code builder to append to.</param>
        /// <param name="quest">The quest blueprint.</param>
        public void Generate(ICodeBuilder builder, QuestBlueprint quest)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (quest == null)
                throw new ArgumentNullException(nameof(quest));

            builder.AppendLines(
                "// ===============================================",
                "// Schedule1ModdingTool generated quest blueprint",
                $"// Mod: {quest.ModName} v{quest.ModVersion} by {quest.ModAuthor}",
                $"// Game: {quest.GameDeveloper} - {quest.GameName}",
                "// ===============================================",
                ""
            );
        }
    }
}
