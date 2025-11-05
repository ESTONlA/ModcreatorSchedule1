using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Schedule1ModdingTool.Models;

namespace Schedule1ModdingTool.Services
{
    /// <summary>
    /// Service for generating C# code and compiling to DLL
    /// </summary>
    public class CodeGenerationService
    {
        public string GenerateQuestCode(QuestBlueprint quest)
        {
            var sb = new StringBuilder();

            // Assembly attributes for MelonLoader
            sb.AppendLine($"[assembly: MelonInfo(typeof({quest.Namespace}.{quest.ClassName}), \"{EscapeString(quest.ModName)}\", \"{EscapeString(quest.ModVersion)}\", \"{EscapeString(quest.ModAuthor)}\")]");
            sb.AppendLine($"[assembly: MelonGame(\"{EscapeString(quest.GameDeveloper)}\", \"{EscapeString(quest.GameName)}\")]");
            sb.AppendLine();

            // Using statements
            sb.AppendLine("using MelonLoader;");
            sb.AppendLine("using S1API.Quests;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();

            // Namespace
            sb.AppendLine($"namespace {quest.Namespace}");
            sb.AppendLine("{");

            // Class documentation
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// MelonLoader Mod: {quest.ModName}");
            sb.AppendLine($"    /// Quest: {quest.QuestTitle}");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public class {quest.ClassName} : MelonMod");
            sb.AppendLine("    {");

            // Quest instance field
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// The quest instance");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine($"        private {quest.QuestId}Quest? questInstance;");
            sb.AppendLine();

            // OnApplicationStart method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Called when the mod is loaded");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public override void OnApplicationStart()");
            sb.AppendLine("        {");
            sb.AppendLine("            LoggerInstance.Msg(\"Mod loaded successfully!\");");
            sb.AppendLine("            LoggerInstance.Msg($\"Registering quest: {0}\", questInstance?.Title ?? \"Unknown\");");
            sb.AppendLine("        }");
            sb.AppendLine();

            // OnSceneWasLoaded method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Called when a scene is loaded - register the quest here");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public override void OnSceneWasLoaded(int buildindex, string sceneName)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Register quest when game scene loads");
            sb.AppendLine("            if (sceneName.Contains(\"GameScene\") || sceneName.Contains(\"Main\"))");
            sb.AppendLine("            {");
            sb.AppendLine($"                questInstance = new {quest.QuestId}Quest();");
            sb.AppendLine("                QuestManager.RegisterQuest(questInstance);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Quest class
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// Quest class: {quest.QuestTitle}");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine($"        public class {quest.QuestId}Quest : Quest");
            sb.AppendLine("        {");

            // Title property
            sb.AppendLine("            /// <summary>");
            sb.AppendLine("            /// The title of the quest");
            sb.AppendLine("            /// </summary>");
            sb.AppendLine($"            protected override string Title => \"{EscapeString(quest.QuestTitle)}\";");
            sb.AppendLine();

            // Description property
            sb.AppendLine("            /// <summary>");
            sb.AppendLine("            /// The description of the quest");
            sb.AppendLine("            /// </summary>");
            sb.AppendLine($"            protected override string Description => \"{EscapeString(quest.QuestDescription)}\";");
            sb.AppendLine();

            // AutoBegin override if needed
            if (!quest.AutoBegin)
            {
                sb.AppendLine("            /// <summary>");
                sb.AppendLine("            /// Override AutoBegin behavior");
                sb.AppendLine("            /// </summary>");
                sb.AppendLine("            protected override bool AutoBegin => false;");
                sb.AppendLine();
            }

            // Custom icon override if needed
            if (quest.CustomIcon)
            {
                sb.AppendLine("            /// <summary>");
                sb.AppendLine("            /// Custom quest icon");
                sb.AppendLine("            /// </summary>");
                sb.AppendLine("            protected override Sprite? QuestIcon => ImageUtils.LoadImage(\"custom_icon.png\");");
                sb.AppendLine();
            }

            // Quest entry fields
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];
                sb.AppendLine($"            /// <summary>");
                sb.AppendLine($"            /// Quest entry: {objective.Title}");
                sb.AppendLine($"            /// </summary>");
                sb.AppendLine($"            private QuestEntry? _{objective.Name};");
                sb.AppendLine();
            }

            // OnCreated method
            sb.AppendLine("            /// <summary>");
            sb.AppendLine("            /// Called when the quest is created");
            sb.AppendLine("            /// </summary>");
            sb.AppendLine("            protected override void OnCreated()");
            sb.AppendLine("            {");

            // Add quest entries
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];
                sb.AppendLine($"                // Add quest entry: {objective.Title}");
                
                if (objective.HasLocation)
                {
                    sb.AppendLine($"                _{objective.Name} = AddEntry(\"{EscapeString(objective.Title)}\", new Vector3({objective.LocationX}f, {objective.LocationY}f, {objective.LocationZ}f));");
                }
                else
                {
                    sb.AppendLine($"                _{objective.Name} = AddEntry(\"{EscapeString(objective.Title)}\");");
                }
                sb.AppendLine();
            }

            // Add reward logic if enabled
            if (quest.QuestRewards)
            {
                sb.AppendLine("                // Set up rewards on individual quest entries");
                foreach (var objective in quest.Objectives)
                {
                    sb.AppendLine($"                _{objective.Name}?.SetReward(() => {{");
                    sb.AppendLine("                    // Add your reward logic here");
                    sb.AppendLine("                    // Example: Player.AddMoney(100);");
                    sb.AppendLine("                });");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("            }");
            sb.AppendLine("        }"); // End Quest class
            sb.AppendLine("    }"); // End MelonMod class
            sb.AppendLine("}"); // End namespace

            return sb.ToString();
        }

        public bool CompileToDll(QuestBlueprint quest, string code)
        {
            try
            {
                // Create output directory on desktop
                var outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Schedule1_Mods");
                Directory.CreateDirectory(outputDir);
                
                // Save the C# source code file
                var sourceCodePath = Path.Combine(outputDir, $"{quest.ClassName}.cs");
                File.WriteAllText(sourceCodePath, code);
                
                // Create a simple instruction file for manual compilation
                var instructionsPath = Path.Combine(outputDir, "COMPILATION_INSTRUCTIONS.txt");
                var instructions = $@"MelonLoader Mod Compilation Instructions
=====================================

Your mod source code has been generated: {quest.ClassName}.cs

To compile this mod to a DLL:

1. Install Visual Studio with .NET development workload
2. Create a new Class Library (.NET Framework 4.7.2 or .NET 6.0) project
3. Add the following NuGet packages:
   - MelonLoader
   - UnityEngine references (from your game installation)
   - S1API references (from Schedule 1 game folder)

4. Replace the default code with your generated code
5. Build the project to create {quest.ClassName}.dll
6. Place the DLL in your MelonLoader Mods folder

Alternative: Use an IDE like Visual Studio Code with the C# extension for a lightweight setup.

Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Mod Name: {quest.ModName}
Version: {quest.ModVersion}
Author: {quest.ModAuthor}
";
                File.WriteAllText(instructionsPath, instructions);
                
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save mod files: {ex.Message}");
            }
        }

        private static string EscapeString(string input)
        {
            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }
    }
}