using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.Services
{
    /// <summary>
    /// Generates strongly-typed quest source code that targets the S1API surface area.
    /// </summary>
    public class CodeGenerationService
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public string GenerateQuestCode(QuestBlueprint quest)
        {
            if (quest == null) throw new ArgumentNullException(nameof(quest));

            var className = MakeSafeIdentifier(quest.ClassName, "GeneratedQuest");
            var sb = new StringBuilder();

            AppendHeader(sb, quest);
            AppendUsings(sb);

            var targetNamespace = NormalizeNamespace(quest.Namespace);
            sb.AppendLine($"namespace {targetNamespace}");
            sb.AppendLine("{");

            AppendQuestClass(sb, quest, className);
            sb.AppendLine();
            AppendRegistryClass(sb, className);

            sb.AppendLine("}");

            return sb.ToString();
        }

        public string GenerateNpcCode(NpcBlueprint npc)
        {
            if (npc == null) throw new ArgumentNullException(nameof(npc));

            var className = MakeSafeIdentifier(npc.ClassName, "GeneratedNpc");
            var sb = new StringBuilder();

            AppendNpcHeader(sb, npc);
            AppendNpcUsings(sb);

            var targetNamespace = NormalizeNamespace(npc.Namespace);
            sb.AppendLine($"namespace {targetNamespace}");
            sb.AppendLine("{");

            AppendNpcClass(sb, npc, className);

            sb.AppendLine("}");
            return sb.ToString();
        }

        public bool CompileToDll(QuestBlueprint quest, string code)
        {
            try
            {
                // Use Roslyn to validate syntax for now (actual compilation requires Unity/S1API refs at export time)
                _ = CSharpSyntaxTree.ParseText(code);
                System.Diagnostics.Debug.WriteLine($"Quest '{quest.ClassName}' validated for export.");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Code validation error: {ex.Message}");
                return false;
            }
        }

        private static void AppendHeader(StringBuilder sb, QuestBlueprint quest)
        {
            sb.AppendLine("// ===============================================");
            sb.AppendLine("// Schedule1ModdingTool generated quest blueprint");
            sb.AppendLine($"// Mod: {quest.ModName} v{quest.ModVersion} by {quest.ModAuthor}");
            sb.AppendLine($"// Game: {quest.GameDeveloper} - {quest.GameName}");
            sb.AppendLine("// ===============================================");
            sb.AppendLine();
        }

        private static void AppendUsings(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using S1API.Quests;");
            sb.AppendLine("using S1API.Saveables;");
            sb.AppendLine("using S1API.Internal.Utils;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using MelonLoader;");
            sb.AppendLine();
        }

        private static void AppendQuestClass(StringBuilder sb, QuestBlueprint quest, string className)
        {
            var questId = string.IsNullOrWhiteSpace(quest.QuestId) ? className : quest.QuestId.Trim();

            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Auto-generated quest blueprint for \"{EscapeString(quest.QuestTitle)}\".");
            sb.AppendLine("    /// Customize the body to wire in game-specific logic.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public class {className} : Quest");
            sb.AppendLine("    {");
            sb.AppendLine($"        public const string QuestIdentifier = \"{EscapeString(questId)}\";");
            sb.AppendLine();

            if (quest.GenerateDataClass)
            {
                sb.AppendLine("        [Serializable]");
                sb.AppendLine("        public class QuestDataModel");
                sb.AppendLine("        {");
                sb.AppendLine("            public bool Completed { get; set; }");
                sb.AppendLine("            // Add additional quest-specific fields here");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine($"        [SaveableField(\"{EscapeString(className)}Data\")]");
                sb.AppendLine("        private QuestDataModel _data = new QuestDataModel();");
                sb.AppendLine();
            }

            sb.AppendLine($"        protected override string Title => \"{EscapeString(quest.QuestTitle)}\";");
            sb.AppendLine($"        protected override string Description => \"{EscapeString(quest.QuestDescription)}\";");
            sb.AppendLine($"        protected override bool AutoBegin => {quest.AutoBegin.ToString().ToLowerInvariant()};");
            if (quest.CustomIcon)
            {
                sb.AppendLine("        protected override Sprite? QuestIcon => LoadCustomIcon();");
            }
            sb.AppendLine();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Call this after creating the quest to set up objectives and tracking.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public void ConfigureQuest()");
            sb.AppendLine("        {");
            sb.AppendLine("            QuestEntries.Clear();");
            sb.AppendLine("            BuildObjectives();");
            if (quest.QuestRewards)
            {
                sb.AppendLine("            // Invoke GrantQuestRewards() once every objective completes.");
            }
            sb.AppendLine("        }");
            sb.AppendLine();

            AppendBuildObjectivesMethod(sb, quest);

            if (quest.QuestRewards)
            {
                AppendRewardStub(sb);
            }

            if (quest.CustomIcon)
            {
                AppendIconStub(sb, quest);
            }

            sb.AppendLine("    }");
        }

        private static void AppendNpcHeader(StringBuilder sb, NpcBlueprint npc)
        {
            sb.AppendLine("// ===============================================");
            sb.AppendLine("// Schedule1ModdingTool generated NPC blueprint");
            sb.AppendLine($"// Mod: {npc.ModName} v{npc.ModVersion} by {npc.ModAuthor}");
            sb.AppendLine($"// Game: {npc.GameDeveloper} - {npc.GameName}");
            sb.AppendLine("// ===============================================");
            sb.AppendLine();
        }

        private static void AppendNpcUsings(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using S1API.Entities;");
            sb.AppendLine("using S1API.Entities.Schedule;");
            sb.AppendLine("using S1API.GameTime;");
            sb.AppendLine("using S1API.Economy;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
        }

        private static void AppendNpcClass(StringBuilder sb, NpcBlueprint npc, string className)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Auto-generated NPC blueprint for \"{EscapeString(npc.DisplayName)}\".");
            sb.AppendLine("    /// Customize ConfigurePrefab and OnCreated to add unique logic.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public sealed class {className} : NPC");
            sb.AppendLine("    {");
            sb.AppendLine($"        public override bool IsPhysical => {npc.IsPhysical.ToString().ToLowerInvariant()};");
            if (npc.IsDealer)
            {
                sb.AppendLine($"        public override bool IsDealer => true;");
            }
            sb.AppendLine();
            sb.AppendLine("        protected override void ConfigurePrefab(NPCPrefabBuilder builder)");
            sb.AppendLine("        {");
            sb.AppendLine($"            builder.WithIdentity(\"{EscapeString(npc.NpcId)}\", \"{EscapeString(npc.FirstName)}\", \"{EscapeString(npc.LastName)}\")");
            AppendAppearanceBuilder(sb, npc.Appearance);

            if (npc.HasSpawnPosition)
            {
                sb.AppendLine($"                .WithSpawnPosition({FormatVector(npc.SpawnX, npc.SpawnY, npc.SpawnZ)})");
            }

            if (npc.EnableCustomer)
            {
                sb.AppendLine("                .EnsureCustomer()");
                sb.AppendLine("                .WithCustomerDefaults(cd =>");
                sb.AppendLine("                {");
                sb.AppendLine("                    cd.WithSpending(400f, 900f)");
                sb.AppendLine("                      .WithOrdersPerWeek(1, 3)");
                sb.AppendLine("                      .WithPreferredOrderDay(Day.Monday)");
                sb.AppendLine("                      .WithOrderTime(900)");
                sb.AppendLine("                      .WithStandards(CustomerStandard.Moderate)");
                sb.AppendLine("                      .AllowDirectApproach(true);");
                sb.AppendLine("                })");
            }

            if (npc.IsDealer)
            {
                sb.AppendLine("                .EnsureDealer()");
                sb.AppendLine("                .WithDealerDefaults(dd =>");
                sb.AppendLine("                {");
                sb.AppendLine("                    dd.WithSigningFee(1000f)");
                sb.AppendLine("                      .WithCut(0.15f)");
                sb.AppendLine("                      .WithDealerType(DealerType.PlayerDealer);");
                sb.AppendLine("                })");
            }

            sb.AppendLine("                ;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {className}() : base(");
            sb.AppendLine($"            id: \"{EscapeString(npc.NpcId)}\",");
            sb.AppendLine($"            firstName: \"{EscapeString(npc.FirstName)}\",");
            sb.AppendLine($"            lastName: \"{EscapeString(npc.LastName)}\",");
            sb.AppendLine("            icon: null)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected override void OnCreated()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.OnCreated();");
            sb.AppendLine("            Appearance.Build();");
            sb.AppendLine("            Schedule.Enable();");
            sb.AppendLine("            Schedule.InitializeActions();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        private static void AppendAppearanceBuilder(StringBuilder sb, NpcAppearanceSettings appearance)
        {
            if (appearance == null)
            {
                sb.AppendLine("                .WithAppearanceDefaults(av => { })");
                return;
            }

            sb.AppendLine("                .WithAppearanceDefaults(av =>");
            sb.AppendLine("                {");
            sb.AppendLine($"                    av.Gender = {FormatFloat(appearance.Gender)}f;");
            sb.AppendLine($"                    av.Height = {FormatFloat(appearance.Height)}f;");
            sb.AppendLine($"                    av.Weight = {FormatFloat(appearance.Weight)}f;");
            sb.AppendLine($"                    av.SkinColor = {ColorUtils.ToUnityColor32Expression(appearance.SkinColor)};");
            sb.AppendLine($"                    av.LeftEyeLidColor = {ColorUtils.ToUnityColorExpression(appearance.LeftEyeLidColor)};");
            sb.AppendLine($"                    av.RightEyeLidColor = {ColorUtils.ToUnityColorExpression(appearance.RightEyeLidColor)};");
            sb.AppendLine($"                    av.EyeBallTint = {ColorUtils.ToUnityColorExpression(appearance.EyeBallTint)};");
            sb.AppendLine($"                    av.HairColor = {ColorUtils.ToUnityColorExpression(appearance.HairColor)};");
            sb.AppendLine($"                    av.HairPath = \"{EscapeString(appearance.HairPath)}\";");
            sb.AppendLine($"                    av.EyeballMaterialIdentifier = \"{EscapeString(appearance.EyeballMaterialIdentifier)}\";");
            sb.AppendLine($"                    av.PupilDilation = {FormatFloat(appearance.PupilDilation)}f;");
            sb.AppendLine($"                    av.EyebrowScale = {FormatFloat(appearance.EyebrowScale)}f;");
            sb.AppendLine($"                    av.EyebrowThickness = {FormatFloat(appearance.EyebrowThickness)}f;");
            sb.AppendLine($"                    av.EyebrowRestingHeight = {FormatFloat(appearance.EyebrowRestingHeight)}f;");
            sb.AppendLine($"                    av.EyebrowRestingAngle = {FormatFloat(appearance.EyebrowRestingAngle)}f;");
            sb.AppendLine($"                    av.LeftEye = {ColorUtils.ToTupleExpression((float)appearance.LeftEyeTop, (float)appearance.LeftEyeBottom)};");
            sb.AppendLine($"                    av.RightEye = {ColorUtils.ToTupleExpression((float)appearance.RightEyeTop, (float)appearance.RightEyeBottom)};");

            foreach (var layer in appearance.FaceLayers)
            {
                sb.AppendLine($"                    av.WithFaceLayer(\"{EscapeString(layer.LayerPath)}\", {ColorUtils.ToUnityColorExpression(layer.ColorHex)});");
            }
            foreach (var layer in appearance.BodyLayers)
            {
                sb.AppendLine($"                    av.WithBodyLayer(\"{EscapeString(layer.LayerPath)}\", {ColorUtils.ToUnityColorExpression(layer.ColorHex)});");
            }
            foreach (var layer in appearance.AccessoryLayers)
            {
                sb.AppendLine($"                    av.WithAccessoryLayer(\"{EscapeString(layer.LayerPath)}\", {ColorUtils.ToUnityColorExpression(layer.ColorHex)});");
            }

            sb.AppendLine("                })");
        }

        private static void AppendBuildObjectivesMethod(StringBuilder sb, QuestBlueprint quest)
        {
            sb.AppendLine("        private void BuildObjectives()");
            sb.AppendLine("        {");

            if (quest.Objectives?.Any() != true)
            {
                sb.AppendLine("            // Define at least one objective so the quest has progress steps.");
                sb.AppendLine("            var defaultEntry = AddEntry(\"Describe your first objective\");");
            }
            else
            {
                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int index = 0;
                foreach (var objective in quest.Objectives)
                {
                    index++;
                    var safeVariable = EnsureUniqueIdentifier(
                        MakeSafeIdentifier(objective.Name, $"objective{index}"),
                        usedNames,
                        index);

                    sb.AppendLine($"            // Objective \"{EscapeString(objective.Title)}\" ({objective.Name})");
                    sb.AppendLine($"            var {safeVariable} = AddEntry(\"{EscapeString(objective.Title)}\");");
                    if (objective.HasLocation)
                    {
                        sb.AppendLine($"            {safeVariable}.POIPosition = {FormatVector(objective)};");
                    }
                    sb.AppendLine($"            // Required progress: {objective.RequiredProgress}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void AppendRewardStub(StringBuilder sb)
        {
            sb.AppendLine("        private void GrantQuestRewards()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Leverage S1API economy/registry helpers to award cash, XP, or items.");
            sb.AppendLine("            // Example: EconomyManager.AddMoney(500);");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void AppendIconStub(StringBuilder sb, QuestBlueprint quest)
        {
            sb.AppendLine("        private Sprite? LoadCustomIcon()");
            sb.AppendLine("        {");
            
            if (string.IsNullOrWhiteSpace(quest.IconFileName))
            {
                sb.AppendLine("            // No icon file specified. Add a resource and select it in the quest settings.");
                sb.AppendLine("            return null;");
            }
            else
            {
                // Convert relative path to resource name format
                // e.g., "Resources/my-icon.png" -> "Namespace.Resources.my-icon.png" or "Namespace.my-icon.png"
                var resourceName = GenerateResourceName(quest.IconFileName, quest.Namespace);
                var fileName = System.IO.Path.GetFileName(quest.IconFileName);
                var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
                
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                var assembly = Assembly.GetExecutingAssembly();");
                sb.AppendLine();
                sb.AppendLine("                // Try different possible resource names");
                sb.AppendLine($"                string[] possibleNames = {{");
                sb.AppendLine($"                    \"{resourceName}\",");
                sb.AppendLine($"                    \"{quest.Namespace}.{fileName}\",");
                sb.AppendLine($"                    \"{fileName}\"");
                sb.AppendLine("                };");
                sb.AppendLine();
                sb.AppendLine("                foreach (string resourceName in possibleNames)");
                sb.AppendLine("                {");
                sb.AppendLine("                    using var stream = assembly.GetManifestResourceStream(resourceName);");
                sb.AppendLine("                    if (stream != null)");
                sb.AppendLine("                    {");
                sb.AppendLine("                        byte[] data = new byte[stream.Length];");
                sb.AppendLine("                        stream.Read(data, 0, data.Length);");
                sb.AppendLine("                        return ImageUtils.LoadImageRaw(data);");
                sb.AppendLine("                    }");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine($"                MelonLogger.Msg($\"Failed to load quest icon '{EscapeString(quest.IconFileName)}': {{ex.Message}}\");");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return null;");
            }
            
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static string GenerateResourceName(string relativePath, string namespaceName)
        {
            // Convert "Resources/my-icon.png" to "Namespace.Resources.my-icon.png"
            var normalizedPath = relativePath.Replace('\\', '/').TrimStart('/');
            var segments = normalizedPath.Split('/');
            var resourceSegments = new List<string> { namespaceName };
            resourceSegments.AddRange(segments);
            return string.Join(".", resourceSegments);
        }

        private static void AppendRegistryClass(StringBuilder sb, string className)
        {
            sb.AppendLine($"    public static class {className}Registration");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static {className}? Register()");
            sb.AppendLine("        {");
            sb.AppendLine($"            var quest = QuestManager.CreateQuest<{className}>({className}.QuestIdentifier) as {className};");
            sb.AppendLine("            quest?.ConfigureQuest();");
            sb.AppendLine("            return quest;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        private static string NormalizeNamespace(string? namespaceValue)
        {
            if (string.IsNullOrWhiteSpace(namespaceValue))
            {
                return "Schedule1Mods.Quests";
            }

            var segments = namespaceValue.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return "Schedule1Mods.Quests";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < segments.Length; i++)
            {
                if (builder.Length > 0)
                {
                    builder.Append('.');
                }

                var fallback = i == 0 ? "Schedule1Mods" : "Generated";
                builder.Append(MakeSafeIdentifier(segments[i], fallback));
            }

            return builder.ToString();
        }

        private static string MakeSafeIdentifier(string? candidate, string fallback)
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
                    if (char.IsLetter(ch) || ch == '_')
                    {
                        builder.Append(ch);
                    }
                    else if (char.IsDigit(ch))
                    {
                        builder.Append('_').Append(ch);
                    }
                }
                else
                {
                    if (char.IsLetterOrDigit(ch) || ch == '_')
                    {
                        builder.Append(ch);
                    }
                    else
                    {
                        builder.Append('_');
                    }
                }
            }

            var result = builder.ToString();
            return string.IsNullOrEmpty(result) ? fallback : result;
        }

        private static string EnsureUniqueIdentifier(string identifier, HashSet<string> usedNames, int index)
        {
            var baseName = string.IsNullOrWhiteSpace(identifier) ? $"objective{index}" : identifier;
            var uniqueName = baseName;
            var suffix = 1;

            while (!usedNames.Add(uniqueName))
            {
                uniqueName = $"{baseName}_{suffix++}";
            }

            return uniqueName;
        }

        private static string FormatVector(QuestObjective objective)
        {
            if (!objective.HasLocation)
            {
                return "Vector3.zero";
            }

            return FormatVector(objective.LocationX, objective.LocationY, objective.LocationZ);
        }

        private static string FormatVector(double x, double y, double z)
        {
            var fx = FormatFloat(x);
            var fy = FormatFloat(y);
            var fz = FormatFloat(z);
            return $"new Vector3({fx}f, {fy}f, {fz}f)";
        }

        private static string FormatFloat(double value)
        {
            return value.ToString("0.###", InvariantCulture);
        }

        private static string EscapeString(string? input)
        {
            return input?
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") ?? string.Empty;
        }
    }
}
