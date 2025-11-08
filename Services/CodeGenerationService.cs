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
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using S1API.Quests;");
            sb.AppendLine("using S1API.Quests.Constants;");
            sb.AppendLine("using S1API.Saveables;");
            sb.AppendLine("using S1API.Internal.Utils;");
            sb.AppendLine("using S1API.Entities;");
            sb.AppendLine("using S1API.GameTime;");
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

            // Generate quest entry field declarations
            AppendQuestEntryFields(sb, quest);

            // Generate OnCreated() override instead of ConfigureQuest()
            AppendOnCreatedMethod(sb, quest);

            if (quest.QuestRewards)
            {
                AppendRewardStub(sb);
            }

            if (quest.CustomIcon)
            {
                AppendIconStub(sb, quest);
            }

            AppendTriggerHandlerFields(sb, quest, className);
            AppendTriggerSubscriptions(sb, quest, className);

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

        private static void AppendQuestEntryFields(StringBuilder sb, QuestBlueprint quest)
        {
            if (quest.Objectives?.Any() != true)
                return;

            sb.AppendLine("        // Quest entry fields for objectives");
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int index = 0;
            foreach (var objective in quest.Objectives)
            {
                index++;
                var safeVariable = EnsureUniqueIdentifier(
                    MakeSafeIdentifier(objective.Name, $"objective{index}"),
                    usedNames,
                    index);
                sb.AppendLine($"        private QuestEntry {safeVariable};");
            }
            sb.AppendLine();
        }

        private static void AppendOnCreatedMethod(StringBuilder sb, QuestBlueprint quest)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Called when the quest is created. Sets up objectives, POI positions, and activates entries.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        protected override void OnCreated()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.OnCreated();");
            sb.AppendLine();

            if (quest.Objectives?.Any() != true)
            {
                sb.AppendLine("            // Define at least one objective so the quest has progress steps.");
                sb.AppendLine("            var defaultEntry = AddEntry(\"Describe your first objective\");");
                sb.AppendLine("            defaultEntry.Begin();");
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
                    
                    // Create entry
                    sb.AppendLine($"            {safeVariable} = AddEntry(\"{EscapeString(objective.Title)}\");");
                    
                    // Set POI position if objective has a location
                    if (objective.HasLocation)
                    {
                        sb.AppendLine($"            {safeVariable}.POIPosition = {FormatVector(objective)};");
                    }

                    // Determine if entry should start active or inactive
                    // If objective has start triggers, set to Inactive; otherwise Begin() immediately
                    if (objective.StartTriggers?.Any() == true)
                    {
                        sb.AppendLine($"            {safeVariable}.SetState(QuestState.Inactive);");
                        sb.AppendLine($"            // Entry will be activated by start trigger");
                    }
                    else
                    {
                        sb.AppendLine($"            {safeVariable}.Begin();");
                    }
                    sb.AppendLine($"            // Required progress: {objective.RequiredProgress}");
                    sb.AppendLine();
                }
            }

            // Check if we have NPC triggers that need delayed subscription
            var hasNpcTriggers = HasNpcTriggers(quest);
            
            if (hasNpcTriggers)
            {
                // NPCs aren't available in OnCreated(), so delay subscription
                // Use a coroutine to wait a bit for NPCs to spawn, then subscribe
                sb.AppendLine("            // NPCs may not be available yet, delay subscription");
                sb.AppendLine("            MelonCoroutines.Start(DelayedTriggerSubscription());");
            }
            else
            {
                // No NPC triggers, can subscribe immediately
                sb.AppendLine("            SubscribeToTriggers();");
            }
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Add delayed subscription coroutine if we have NPC triggers
            if (hasNpcTriggers)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// Delays trigger subscription to ensure NPCs are available.");
                sb.AppendLine("        /// Waits for NPCs to spawn before subscribing to their events.");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        private IEnumerator DelayedTriggerSubscription()");
                sb.AppendLine("        {");
                sb.AppendLine("            // Wait for NPCs to be available (check every 0.5s, up to 30 seconds)");
                sb.AppendLine("            float timeout = 30f;");
                sb.AppendLine("            float waited = 0f;");
                sb.AppendLine("            float checkInterval = 0.5f;");
                sb.AppendLine();
                sb.AppendLine("            while (waited < timeout)");
                sb.AppendLine("            {");
                sb.AppendLine("                // Check if any NPCs are available");
                sb.AppendLine("                if (NPC.All != null && NPC.All.Count > 0)");
                sb.AppendLine("                {");
                sb.AppendLine("                    MelonLogger.Msg($\"[Quest] NPCs available after {waited:F1}s, subscribing to triggers\");");
                sb.AppendLine("                    SubscribeToTriggers();");
                sb.AppendLine("                    yield break;");
                sb.AppendLine("                }");
                sb.AppendLine();
                sb.AppendLine("                yield return new WaitForSeconds(checkInterval);");
                sb.AppendLine("                waited += checkInterval;");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            // Timeout reached, try subscribing anyway (retry logic will handle it)");
                sb.AppendLine("            MelonLogger.Warning($\"[Quest] NPC availability timeout after {timeout}s, attempting subscription anyway\");");
            sb.AppendLine("            SubscribeToTriggers();");
            sb.AppendLine("        }");
            sb.AppendLine();
            }
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
                // Extract just the filename (in case IconFileName includes a path)
                var fileName = System.IO.Path.GetFileName(quest.IconFileName);
                // Construct resource name as ModName.Resources.IconName.png
                var resourceName = $"{quest.ModName}.Resources.{fileName}";
                
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                var assembly = Assembly.GetExecutingAssembly();");
                sb.AppendLine();
                sb.AppendLine($"                using var stream = assembly.GetManifestResourceStream(\"{EscapeString(resourceName)}\");");
                sb.AppendLine("                if (stream != null)");
                sb.AppendLine("                {");
                sb.AppendLine("                    byte[] data = new byte[stream.Length];");
                sb.AppendLine("                    stream.Read(data, 0, data.Length);");
                sb.AppendLine("                    return ImageUtils.LoadImageRaw(data);");
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

        private static void AppendTriggerHandlerFields(StringBuilder sb, QuestBlueprint quest, string className)
        {
            var handlerFields = CollectTriggerHandlers(quest);
            if (handlerFields.Count == 0)
                return;

            sb.AppendLine("        // Trigger event handlers");
            foreach (var handlerField in handlerFields)
            {
                sb.AppendLine($"        private Action {handlerField.FieldName};");
            }
            sb.AppendLine();
        }

        private static List<TriggerHandlerInfo> CollectTriggerHandlers(QuestBlueprint quest)
        {
            var handlers = new List<TriggerHandlerInfo>();
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int handlerIndex = 0;

            // Quest start triggers
            if (quest.QuestTriggers?.Any(t => t.TriggerTarget == QuestTriggerTarget.QuestStart) == true)
            {
                foreach (var trigger in quest.QuestTriggers.Where(t => t.TriggerTarget == QuestTriggerTarget.QuestStart))
                {
                    if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetAction))
                    {
                        var handlerName = GenerateHandlerFieldName(trigger, usedNames, ref handlerIndex);
                        handlers.Add(new TriggerHandlerInfo { Trigger = trigger, FieldName = handlerName, ActionMethod = "Begin()" });
                    }
                }
            }

            // Quest finish triggers
            if (quest.QuestFinishTriggers?.Any() == true)
            {
                foreach (var trigger in quest.QuestFinishTriggers)
                {
                    if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetAction))
                    {
                        var finishMethod = trigger.FinishType switch
                        {
                            QuestFinishType.Complete => "Complete()",
                            QuestFinishType.Fail => "Fail()",
                            QuestFinishType.Cancel => "Cancel()",
                            QuestFinishType.Expire => "Expire()",
                            _ => "Complete()"
                        };
                        var handlerName = GenerateHandlerFieldName(trigger, usedNames, ref handlerIndex);
                        handlers.Add(new TriggerHandlerInfo { Trigger = trigger, FieldName = handlerName, ActionMethod = finishMethod });
                    }
                }
            }

            // Objective triggers
            if (quest.Objectives?.Any() == true)
            {
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    var objective = quest.Objectives[i];

                    // Objective start triggers
                    if (objective.StartTriggers?.Any() == true)
                    {
                        foreach (var trigger in objective.StartTriggers)
                        {
                            if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetAction))
                            {
                                var handlerName = GenerateHandlerFieldName(trigger, usedNames, ref handlerIndex);
                                handlers.Add(new TriggerHandlerInfo { Trigger = trigger, FieldName = handlerName, ActionMethod = "Begin()", ObjectiveIndex = i });
                            }
                        }
                    }

                    // Objective finish triggers
                    if (objective.FinishTriggers?.Any() == true)
                    {
                        foreach (var trigger in objective.FinishTriggers)
                        {
                            if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetAction))
                            {
                                var handlerName = GenerateHandlerFieldName(trigger, usedNames, ref handlerIndex);
                                handlers.Add(new TriggerHandlerInfo { Trigger = trigger, FieldName = handlerName, ActionMethod = "Complete()", ObjectiveIndex = i });
                            }
                        }
                    }
                }
            }

            return handlers;
        }

        private static string GenerateHandlerFieldName(QuestTrigger trigger, HashSet<string> usedNames, ref int handlerIndex)
        {
            var actionParts = trigger.TargetAction.Split('.');
            var eventName = actionParts.Length >= 2 ? actionParts[1] : trigger.TargetAction;
            var baseName = $"_{MakeSafeIdentifier(eventName, "trigger")}Handler";
            var uniqueName = EnsureUniqueIdentifier(baseName, usedNames, ++handlerIndex);
            return uniqueName;
        }

        private static string GenerateHandlerFieldName(QuestObjectiveTrigger trigger, HashSet<string> usedNames, ref int handlerIndex)
        {
            var actionParts = trigger.TargetAction.Split('.');
            var eventName = actionParts.Length >= 2 ? actionParts[1] : trigger.TargetAction;
            var baseName = $"_{MakeSafeIdentifier(eventName, "trigger")}Handler";
            var uniqueName = EnsureUniqueIdentifier(baseName, usedNames, ++handlerIndex);
            return uniqueName;
        }

        private static bool HasNpcTriggers(QuestBlueprint quest)
        {
            // Check quest start triggers
            if (quest.QuestTriggers?.Any(t => t.TriggerType == QuestTriggerType.NPCEventTrigger && 
                                             t.TriggerTarget == QuestTriggerTarget.QuestStart && 
                                             !string.IsNullOrWhiteSpace(t.TargetNpcId)) == true)
                return true;

            // Check quest finish triggers
            if (quest.QuestFinishTriggers?.Any(t => t.TriggerType == QuestTriggerType.NPCEventTrigger && 
                                                    !string.IsNullOrWhiteSpace(t.TargetNpcId)) == true)
                return true;

            // Check objective triggers
            if (quest.Objectives?.Any(o => 
                (o.StartTriggers?.Any(t => t.TriggerType == QuestTriggerType.NPCEventTrigger && 
                                          !string.IsNullOrWhiteSpace(t.TargetNpcId)) == true) ||
                (o.FinishTriggers?.Any(t => t.TriggerType == QuestTriggerType.NPCEventTrigger && 
                                           !string.IsNullOrWhiteSpace(t.TargetNpcId)) == true)) == true)
                return true;

            return false;
        }

        private static void AppendTriggerSubscriptions(StringBuilder sb, QuestBlueprint quest, string className)
        {
            var questId = string.IsNullOrWhiteSpace(quest.QuestId) ? className : quest.QuestId.Trim();
            var hasTriggers = (quest.QuestTriggers?.Any() == true) || 
                             (quest.QuestFinishTriggers?.Any() == true) ||
                             (quest.Objectives?.Any(o => o.StartTriggers?.Any() == true || o.FinishTriggers?.Any() == true) == true);

            var handlerMap = BuildHandlerMap(quest);
            var hasNpcTriggers = HasNpcTriggers(quest);

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Subscribes to triggers for this quest and its objectives.");
            if (hasNpcTriggers)
            {
                sb.AppendLine("        /// Called when the quest begins (via onQuestBegin) to ensure NPCs are available.");
            }
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private void SubscribeToTriggers()");
            sb.AppendLine("        {");
            
            if (!hasTriggers)
            {
                sb.AppendLine("            // No triggers configured for this quest");
                sb.AppendLine("        }");
                sb.AppendLine();
                return;
            }

            // Quest start triggers
            if (quest.QuestTriggers?.Any(t => t.TriggerTarget == QuestTriggerTarget.QuestStart) == true)
            {
                foreach (var trigger in quest.QuestTriggers.Where(t => t.TriggerTarget == QuestTriggerTarget.QuestStart))
                {
                    var key = GetTriggerKey(trigger, null);
                    var handlerInfo = handlerMap.GetValueOrDefault(key);
                    AppendTriggerSubscription(sb, trigger, className, questId, handlerInfo?.FieldName, handlerInfo?.ActionMethod ?? "Begin()");
                }
            }

            // Quest finish triggers
            if (quest.QuestFinishTriggers?.Any() == true)
            {
                foreach (var trigger in quest.QuestFinishTriggers)
                {
                    var finishMethod = trigger.FinishType switch
                    {
                        QuestFinishType.Complete => "Complete()",
                        QuestFinishType.Fail => "Fail()",
                        QuestFinishType.Cancel => "Cancel()",
                        QuestFinishType.Expire => "Expire()",
                        _ => "Complete()"
                    };
                    var key = GetTriggerKey(trigger, null);
                    var handlerInfo = handlerMap.GetValueOrDefault(key);
                    AppendTriggerSubscription(sb, trigger, className, questId, handlerInfo?.FieldName, finishMethod);
                }
            }

            // Objective triggers
            if (quest.Objectives?.Any() == true)
            {
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    var objective = quest.Objectives[i];
                    var objectiveVar = $"objective_{i + 1}";

                    // Find the objective variable name from BuildObjectives
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    int objIndex = 0;
                    foreach (var obj in quest.Objectives)
                    {
                        objIndex++;
                        var safeVar = EnsureUniqueIdentifier(
                            MakeSafeIdentifier(obj.Name, $"objective{objIndex}"),
                            usedNames,
                            objIndex);
                        if (obj == objective)
                        {
                            objectiveVar = safeVar;
                            break;
                        }
                    }

                    // Objective start triggers - activate entry when triggered
                    if (objective.StartTriggers?.Any() == true)
                    {
                        foreach (var trigger in objective.StartTriggers)
                        {
                            var key = GetTriggerKey(trigger, i);
                            var handlerInfo = handlerMap.GetValueOrDefault(key);
                            // Start triggers should call Begin() to activate the entry
                            AppendObjectiveTriggerSubscription(sb, trigger, className, objectiveVar, handlerInfo?.FieldName, "Begin()", handlerInfo?.ObjectiveIndex ?? i);
                        }
                    }

                    // Objective finish triggers
                    if (objective.FinishTriggers?.Any() == true)
                    {
                        foreach (var trigger in objective.FinishTriggers)
                        {
                            var key = GetTriggerKey(trigger, i);
                            var handlerInfo = handlerMap.GetValueOrDefault(key);
                            AppendObjectiveTriggerSubscription(sb, trigger, className, objectiveVar, handlerInfo?.FieldName, handlerInfo?.ActionMethod ?? "Complete()", handlerInfo?.ObjectiveIndex ?? i);
                        }
                    }
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Add retry helper methods if we have NPC triggers
            if (hasNpcTriggers)
            {
                AppendNpcRetryHelpers(sb);
            }
        }

        private static void AppendNpcRetryHelpers(StringBuilder sb)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Retries subscribing to an NPC trigger if the NPC wasn't found immediately.");
            sb.AppendLine("        /// Keeps retrying indefinitely until the NPC is found.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private IEnumerator SubscribeToNpcTriggerWithRetry(string npcId, string componentType, string eventName, Action handler)");
            sb.AppendLine("        {");
            sb.AppendLine("            float retryDelay = 0.5f;");
            sb.AppendLine("            int attemptCount = 0;");
            sb.AppendLine();
            sb.AppendLine("            while (true)");
            sb.AppendLine("            {");
            sb.AppendLine("                attemptCount++;");
            sb.AppendLine("                var npc = NPC.All.FirstOrDefault(n => n.ID == npcId);");
            sb.AppendLine("                if (npc != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    // Subscribe using the same pattern as SubscribeToTriggers()");
            sb.AppendLine("                    if (componentType == \"NPCCustomer\")");
            sb.AppendLine("                    {");
            sb.AppendLine("                        switch (eventName)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            case \"OnDealCompleted\":");
            sb.AppendLine("                                npc.Customer.OnDealCompleted -= handler;");
            sb.AppendLine("                                npc.Customer.OnDealCompleted += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                            case \"OnUnlocked\":");
            sb.AppendLine("                                npc.Customer.OnUnlocked -= handler;");
            sb.AppendLine("                                npc.Customer.OnUnlocked += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                            case \"OnContractAssigned\":");
            sb.AppendLine("                                // OnContractAssigned has parameters, cannot use simple Action");
            sb.AppendLine("                                MelonLogger.Warning($\"[Quest] OnContractAssigned requires Action<float, int, int, int>, cannot retry subscribe\");");
            sb.AppendLine("                                yield break;");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine("                    else if (componentType == \"NPCDealer\")");
            sb.AppendLine("                    {");
            sb.AppendLine("                        switch (eventName)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            case \"OnRecruited\":");
            sb.AppendLine("                                npc.Dealer.OnRecruited -= handler;");
            sb.AppendLine("                                npc.Dealer.OnRecruited += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                            case \"OnContractAccepted\":");
            sb.AppendLine("                                npc.Dealer.OnContractAccepted -= handler;");
            sb.AppendLine("                                npc.Dealer.OnContractAccepted += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine("                    else");
            sb.AppendLine("                    {");
            sb.AppendLine("                        // Direct NPC events");
            sb.AppendLine("                        switch (eventName)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            case \"OnDeath\":");
            sb.AppendLine("                                npc.OnDeath -= handler;");
            sb.AppendLine("                                npc.OnDeath += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                            case \"OnInventoryChanged\":");
            sb.AppendLine("                                npc.OnInventoryChanged -= handler;");
            sb.AppendLine("                                npc.OnInventoryChanged += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine();
            sb.AppendLine("                    MelonLogger.Msg($\"[Quest] Successfully subscribed to NPC trigger '{npcId}.{componentType}.{eventName}' after {attemptCount} attempts\");");
            sb.AppendLine("                    yield break;");
            sb.AppendLine("                }");
            sb.AppendLine();
            sb.AppendLine("                // Log every 10 attempts to avoid spam");
            sb.AppendLine("                if (attemptCount % 10 == 0)");
            sb.AppendLine("                {");
            sb.AppendLine("                    MelonLogger.Msg($\"[Quest] Still waiting for NPC '{npcId}' to become available (attempt {attemptCount})...\");");
            sb.AppendLine("                }");
            sb.AppendLine();
            sb.AppendLine("                yield return new WaitForSeconds(retryDelay);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Retries subscribing to an NPC objective trigger if the NPC wasn't found immediately.");
            sb.AppendLine("        /// Keeps retrying indefinitely until the NPC is found.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private IEnumerator SubscribeToNpcObjectiveTriggerWithRetry(string npcId, string componentType, string eventName, QuestEntry entry, Action handler)");
            sb.AppendLine("        {");
            sb.AppendLine("            float retryDelay = 0.5f;");
            sb.AppendLine("            int attemptCount = 0;");
            sb.AppendLine();
            sb.AppendLine("            while (true)");
            sb.AppendLine("            {");
            sb.AppendLine("                attemptCount++;");
            sb.AppendLine("                var npc = NPC.All.FirstOrDefault(n => n.ID == npcId);");
            sb.AppendLine("                if (npc != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    // Subscribe using the same pattern as SubscribeToTriggers()");
            sb.AppendLine("                    if (componentType == \"NPCCustomer\")");
            sb.AppendLine("                    {");
            sb.AppendLine("                        switch (eventName)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            case \"OnDealCompleted\":");
            sb.AppendLine("                                npc.Customer.OnDealCompleted -= handler;");
            sb.AppendLine("                                npc.Customer.OnDealCompleted += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                            case \"OnUnlocked\":");
            sb.AppendLine("                                npc.Customer.OnUnlocked -= handler;");
            sb.AppendLine("                                npc.Customer.OnUnlocked += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine("                    else if (componentType == \"NPCDealer\")");
            sb.AppendLine("                    {");
            sb.AppendLine("                        switch (eventName)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            case \"OnRecruited\":");
            sb.AppendLine("                                npc.Dealer.OnRecruited -= handler;");
            sb.AppendLine("                                npc.Dealer.OnRecruited += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                            case \"OnContractAccepted\":");
            sb.AppendLine("                                npc.Dealer.OnContractAccepted -= handler;");
            sb.AppendLine("                                npc.Dealer.OnContractAccepted += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine("                    else");
            sb.AppendLine("                    {");
            sb.AppendLine("                        // Direct NPC events");
            sb.AppendLine("                        switch (eventName)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            case \"OnDeath\":");
            sb.AppendLine("                                npc.OnDeath -= handler;");
            sb.AppendLine("                                npc.OnDeath += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                            case \"OnInventoryChanged\":");
            sb.AppendLine("                                npc.OnInventoryChanged -= handler;");
            sb.AppendLine("                                npc.OnInventoryChanged += handler;");
            sb.AppendLine("                                break;");
            sb.AppendLine("                        }");
            sb.AppendLine("                    }");
            sb.AppendLine();
            sb.AppendLine("                    MelonLogger.Msg($\"[Quest] Successfully subscribed to NPC objective trigger '{npcId}.{componentType}.{eventName}' after {attemptCount} attempts\");");
            sb.AppendLine("                    yield break;");
            sb.AppendLine("                }");
            sb.AppendLine();
            sb.AppendLine("                // Log every 10 attempts to avoid spam");
            sb.AppendLine("                if (attemptCount % 10 == 0)");
            sb.AppendLine("                {");
            sb.AppendLine("                    MelonLogger.Msg($\"[Quest] Still waiting for NPC '{npcId}' to become available (attempt {attemptCount})...\");");
            sb.AppendLine("                }");
            sb.AppendLine();
            sb.AppendLine("                yield return new WaitForSeconds(retryDelay);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static Dictionary<string, TriggerHandlerInfo> BuildHandlerMap(QuestBlueprint quest)
        {
            var map = new Dictionary<string, TriggerHandlerInfo>();
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int handlerIndex = 0;

            // Quest start triggers
            if (quest.QuestTriggers?.Any(t => t.TriggerTarget == QuestTriggerTarget.QuestStart) == true)
            {
                foreach (var trigger in quest.QuestTriggers.Where(t => t.TriggerTarget == QuestTriggerTarget.QuestStart))
                {
                    if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetAction))
                    {
                        var handlerName = GenerateHandlerFieldName(trigger, usedNames, ref handlerIndex);
                        var key = GetTriggerKey(trigger, null);
                        map[key] = new TriggerHandlerInfo { Trigger = trigger, FieldName = handlerName, ActionMethod = "Begin()" };
                    }
                }
            }

            // Quest finish triggers
            if (quest.QuestFinishTriggers?.Any() == true)
            {
                foreach (var finishTrigger in quest.QuestFinishTriggers)
                {
                    if (finishTrigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(finishTrigger.TargetAction))
                    {
                        var finishMethod = finishTrigger.FinishType switch
                        {
                            QuestFinishType.Complete => "Complete()",
                            QuestFinishType.Fail => "Fail()",
                            QuestFinishType.Cancel => "Cancel()",
                            QuestFinishType.Expire => "Expire()",
                            _ => "Complete()"
                        };
                        var handlerName = GenerateHandlerFieldName(finishTrigger, usedNames, ref handlerIndex);
                        var key = GetTriggerKey(finishTrigger, null);
                        map[key] = new TriggerHandlerInfo { Trigger = finishTrigger, FieldName = handlerName, ActionMethod = finishMethod };
                    }
                }
            }

            // Objective triggers
            if (quest.Objectives?.Any() == true)
            {
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    var objective = quest.Objectives[i];

                    // Objective start triggers
                    if (objective.StartTriggers?.Any() == true)
                    {
                        foreach (var trigger in objective.StartTriggers)
                        {
                            if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetAction))
                            {
                                var handlerName = GenerateHandlerFieldName(trigger, usedNames, ref handlerIndex);
                                var key = GetTriggerKey(trigger, i);
                                map[key] = new TriggerHandlerInfo { Trigger = trigger, FieldName = handlerName, ActionMethod = "Begin()", ObjectiveIndex = i };
                            }
                        }
                    }

                    // Objective finish triggers
                    if (objective.FinishTriggers?.Any() == true)
                    {
                        foreach (var trigger in objective.FinishTriggers)
                        {
                            if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetAction))
                            {
                                var handlerName = GenerateHandlerFieldName(trigger, usedNames, ref handlerIndex);
                                var key = GetTriggerKey(trigger, i);
                                map[key] = new TriggerHandlerInfo { Trigger = trigger, FieldName = handlerName, ActionMethod = "Complete()", ObjectiveIndex = i };
                            }
                        }
                    }
                }
            }

            return map;
        }

        private static string GetTriggerKey(QuestTrigger trigger, int? objectiveIndex)
        {
            // Create a unique key based on trigger properties
            var objIdx = objectiveIndex ?? trigger.ObjectiveIndex ?? -1;
            return $"{trigger.TriggerType}|{trigger.TargetAction}|{trigger.TargetNpcId}|{trigger.TriggerTarget}|{objIdx}";
        }

        private class TriggerHandlerInfo
        {
            public QuestTrigger? Trigger { get; set; }
            public string FieldName { get; set; } = string.Empty;
            public string ActionMethod { get; set; } = string.Empty;
            public int? ObjectiveIndex { get; set; }
        }

        private static void AppendTriggerSubscription(StringBuilder sb, QuestTrigger trigger, string className, string questId, string? handlerFieldName, string actionMethod)
        {
            if (string.IsNullOrWhiteSpace(trigger.TargetAction))
                return;

            sb.AppendLine($"            // Trigger: {EscapeString(trigger.TargetAction)} -> {trigger.TriggerTarget}");
            sb.AppendLine("            try");
            sb.AppendLine("            {");

            if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetNpcId))
            {
                // NPC instance event trigger
                var npcId = EscapeString(trigger.TargetNpcId);
                var actionParts = trigger.TargetAction.Split('.');
                string eventPath;
                
                if (actionParts.Length >= 2)
                {
                    var componentType = actionParts[0]; // NPCCustomer, NPCDealer, or NPC
                    var eventName = actionParts[1]; // OnDealCompleted, OnRecruited, etc.
                    
                    if (componentType == "NPCCustomer")
                    {
                        // Customer events: npc.Customer.OnDealCompleted
                        eventPath = $"npc.Customer.{eventName}";
                    }
                    else if (componentType == "NPCDealer")
                    {
                        // Dealer events: npc.Dealer.OnRecruited
                        eventPath = $"npc.Dealer.{eventName}";
                    }
                    else
                    {
                        // Direct NPC events: npc.OnDeath, npc.OnInventoryChanged
                        eventPath = $"npc.{eventName}";
                    }
                }
                else
                {
                    // Fallback to direct NPC event
                    var actionName = trigger.TargetAction.Contains(".") ? trigger.TargetAction.Split('.')[1] : trigger.TargetAction;
                    eventPath = $"npc.{actionName}";
                }

                sb.AppendLine($"                var npc = NPC.All.FirstOrDefault(n => n.ID == \"{npcId}\");");
                sb.AppendLine("                if (npc != null)");
                sb.AppendLine("                {");
                
                if (!string.IsNullOrWhiteSpace(handlerFieldName))
                {
                    // Use Action field pattern like ExamplePhysicalNPC2.cs
                    sb.AppendLine($"                    {handlerFieldName} ??= () =>");
                    sb.AppendLine("                    {");
                    sb.AppendLine($"                        {actionMethod};");
                    sb.AppendLine("                    };");
                    sb.AppendLine($"                    {eventPath} -= {handlerFieldName};");
                    sb.AppendLine($"                    {eventPath} += {handlerFieldName};");
                }
                else
                {
                    // Fallback for non-NPC triggers
                    sb.AppendLine($"                    {eventPath} += () =>");
                    sb.AppendLine("                    {");
                    sb.AppendLine($"                        {actionMethod};");
                    sb.AppendLine("                    };");
                }
                
                sb.AppendLine("                }");
                sb.AppendLine("                else");
                sb.AppendLine("                {");
                sb.AppendLine("                    // NPC not found immediately, retry with coroutine");
                var actionPartsForRetry = trigger.TargetAction.Split('.');
                var componentTypeForRetry = actionPartsForRetry.Length >= 2 ? actionPartsForRetry[0] : "NPC";
                var eventNameForRetry = actionPartsForRetry.Length >= 2 ? actionPartsForRetry[1] : trigger.TargetAction;
                
                if (!string.IsNullOrWhiteSpace(handlerFieldName))
                {
                    sb.AppendLine($"                    MelonCoroutines.Start(SubscribeToNpcTriggerWithRetry(\"{npcId}\", \"{EscapeString(componentTypeForRetry)}\", \"{EscapeString(eventNameForRetry)}\", {handlerFieldName}));");
                }
                else
                {
                    sb.AppendLine($"                    Action retryHandler = () => {{ {actionMethod}; }};");
                    sb.AppendLine($"                    MelonCoroutines.Start(SubscribeToNpcTriggerWithRetry(\"{npcId}\", \"{EscapeString(componentTypeForRetry)}\", \"{EscapeString(eventNameForRetry)}\", retryHandler));");
                }
                sb.AppendLine("                }");
            }
            else
            {
                // Static Action trigger
                var actionParts = trigger.TargetAction.Split('.');
                if (actionParts.Length == 2)
                {
                    var targetClass = actionParts[0];
                    var actionName = actionParts[1];

                    sb.AppendLine($"                {targetClass}.{actionName} += () =>");
                    sb.AppendLine("                {");
                    sb.AppendLine($"                    {actionMethod};");
                    sb.AppendLine("                };");
                }
            }

            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine($"                MelonLogger.Warning($\"Failed to subscribe to trigger {EscapeString(trigger.TargetAction)}: {{ex.Message}}\");");
            sb.AppendLine("            }");
            sb.AppendLine();
        }

        private static void AppendObjectiveTriggerSubscription(StringBuilder sb, QuestObjectiveTrigger trigger, string className, string objectiveVar, string? handlerFieldName, string actionMethod, int objectiveIndex)
        {
            if (string.IsNullOrWhiteSpace(trigger.TargetAction))
                return;

            sb.AppendLine($"            // Objective trigger: {EscapeString(trigger.TargetAction)} -> {objectiveVar}.{actionMethod}");
            sb.AppendLine("            try");
            sb.AppendLine("            {");

            if (trigger.TriggerType == QuestTriggerType.NPCEventTrigger && !string.IsNullOrWhiteSpace(trigger.TargetNpcId))
            {
                // NPC instance event trigger
                var npcId = EscapeString(trigger.TargetNpcId);
                var actionParts = trigger.TargetAction.Split('.');
                string eventPath;
                
                if (actionParts.Length >= 2)
                {
                    var componentType = actionParts[0]; // NPCCustomer, NPCDealer, or NPC
                    var eventName = actionParts[1]; // OnDealCompleted, OnRecruited, etc.
                    
                    if (componentType == "NPCCustomer")
                    {
                        // Customer events: npc.Customer.OnDealCompleted
                        eventPath = $"npc.Customer.{eventName}";
                    }
                    else if (componentType == "NPCDealer")
                    {
                        // Dealer events: npc.Dealer.OnRecruited
                        eventPath = $"npc.Dealer.{eventName}";
                    }
                    else
                    {
                        // Direct NPC events: npc.OnDeath, npc.OnInventoryChanged
                        eventPath = $"npc.{eventName}";
                    }
                }
                else
                {
                    // Fallback to direct NPC event
                    var actionName = trigger.TargetAction.Contains(".") ? trigger.TargetAction.Split('.')[1] : trigger.TargetAction;
                    eventPath = $"npc.{actionName}";
                }

                sb.AppendLine($"                var npc = NPC.All.FirstOrDefault(n => n.ID == \"{npcId}\");");
                sb.AppendLine("                if (npc != null)");
                sb.AppendLine("                {");
                
                if (!string.IsNullOrWhiteSpace(handlerFieldName))
                {
                    // Use Action field pattern like ExamplePhysicalNPC2.cs
                    // Use the entry field reference directly instead of QuestEntries[index]
                    sb.AppendLine($"                    {handlerFieldName} ??= () =>");
                    sb.AppendLine("                    {");
                    sb.AppendLine($"                        if ({objectiveVar} != null)");
                    sb.AppendLine("                        {");
                    sb.AppendLine($"                            {objectiveVar}.{actionMethod};");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                    };");
                    sb.AppendLine($"                    {eventPath} -= {handlerFieldName};");
                    sb.AppendLine($"                    {eventPath} += {handlerFieldName};");
                }
                else
                {
                    // Fallback for non-NPC triggers
                    sb.AppendLine($"                    {eventPath} += () =>");
                    sb.AppendLine("                    {");
                    sb.AppendLine($"                        if ({objectiveVar} != null)");
                    sb.AppendLine("                        {");
                    sb.AppendLine($"                            {objectiveVar}.{actionMethod};");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                    };");
                }
                
                sb.AppendLine("                }");
                sb.AppendLine("                else");
                sb.AppendLine("                {");
                sb.AppendLine("                    // NPC not found immediately, retry with coroutine");
                var actionPartsForObjRetry = trigger.TargetAction.Split('.');
                var componentTypeForObjRetry = actionPartsForObjRetry.Length >= 2 ? actionPartsForObjRetry[0] : "NPC";
                var eventNameForObjRetry = actionPartsForObjRetry.Length >= 2 ? actionPartsForObjRetry[1] : trigger.TargetAction;
                
                if (!string.IsNullOrWhiteSpace(handlerFieldName))
                {
                    sb.AppendLine($"                    MelonCoroutines.Start(SubscribeToNpcObjectiveTriggerWithRetry(\"{npcId}\", \"{EscapeString(componentTypeForObjRetry)}\", \"{EscapeString(eventNameForObjRetry)}\", {objectiveVar}, {handlerFieldName}));");
                }
                else
                {
                    sb.AppendLine($"                    Action retryHandler = () => {{ if ({objectiveVar} != null) {{ {objectiveVar}.{actionMethod}; }} }};");
                    sb.AppendLine($"                    MelonCoroutines.Start(SubscribeToNpcObjectiveTriggerWithRetry(\"{npcId}\", \"{EscapeString(componentTypeForObjRetry)}\", \"{EscapeString(eventNameForObjRetry)}\", {objectiveVar}, retryHandler));");
                }
                sb.AppendLine("                }");
            }
            else
            {
                // Static Action trigger
                var actionParts = trigger.TargetAction.Split('.');
                if (actionParts.Length == 2)
                {
                    var classPart = actionParts[0];
                    var actionName = actionParts[1];

                    sb.AppendLine($"                {classPart}.{actionName} += () =>");
                    sb.AppendLine("                {");
                    sb.AppendLine($"                    if ({objectiveVar} != null)");
                    sb.AppendLine("                    {");
                    sb.AppendLine($"                        {objectiveVar}.{actionMethod};");
                    sb.AppendLine("                    }");
                    sb.AppendLine("                };");
                }
            }

            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine($"                MelonLogger.Warning($\"Failed to subscribe to objective trigger {EscapeString(trigger.TargetAction)}: {{ex.Message}}\");");
            sb.AppendLine("            }");
            sb.AppendLine();
        }
    }
}
