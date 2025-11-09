using System;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services.CodeGeneration.Abstractions;

namespace Schedule1ModdingTool.Services.CodeGeneration.Npc
{
    /// <summary>
    /// Generates file header comments for NPC blueprints.
    /// Creates a standardized header with mod and game information.
    /// </summary>
    public class NpcHeaderGenerator
    {
        /// <summary>
        /// Generates the header comment block for an NPC file.
        /// </summary>
        /// <param name="builder">The code builder to append to.</param>
        /// <param name="npc">The NPC blueprint.</param>
        public void Generate(ICodeBuilder builder, NpcBlueprint npc)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (npc == null)
                throw new ArgumentNullException(nameof(npc));

            builder.AppendLines(
                "// ===============================================",
                "// Schedule1ModdingTool generated NPC blueprint",
                $"// Mod: {npc.ModName} v{npc.ModVersion} by {npc.ModAuthor}",
                $"// Game: {npc.GameDeveloper} - {npc.GameName}",
                "// ===============================================",
                ""
            );
        }
    }
}
