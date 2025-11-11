using System;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services.CodeGeneration.Abstractions;
using Schedule1ModdingTool.Services.CodeGeneration.Builders;
using Schedule1ModdingTool.Services.CodeGeneration.Common;

namespace Schedule1ModdingTool.Services.CodeGeneration.Npc
{
    /// <summary>
    /// Main orchestrator for generating complete NPC source code.
    /// Composes header, appearance, and class structure generators.
    /// </summary>
    public class NpcCodeGenerator : ICodeGenerator<NpcBlueprint>
    {
        private readonly NpcHeaderGenerator _headerGenerator;
        private readonly NpcAppearanceGenerator _appearanceGenerator;
        private readonly NpcScheduleGenerator _scheduleGenerator;

        public NpcCodeGenerator()
        {
            _headerGenerator = new NpcHeaderGenerator();
            _appearanceGenerator = new NpcAppearanceGenerator();
            _scheduleGenerator = new NpcScheduleGenerator();
        }

        /// <summary>
        /// Generates complete C# source code for an NPC blueprint.
        /// </summary>
        public string GenerateCode(NpcBlueprint npc)
        {
            if (npc == null)
                throw new ArgumentNullException(nameof(npc));

            var builder = new CodeBuilder();
            var className = IdentifierSanitizer.MakeSafeIdentifier(npc.ClassName, "GeneratedNpc");
            var targetNamespace = NamespaceNormalizer.NormalizeForNpc(npc.Namespace);

            // File header
            _headerGenerator.Generate(builder, npc);

            // Using statements
            var usingsBuilder = new UsingStatementsBuilder();
            usingsBuilder.AddNpcUsings();
            usingsBuilder.GenerateUsings(builder);

            // Namespace
            builder.OpenBlock($"namespace {targetNamespace}");

            // NPC class
            GenerateNpcClass(builder, npc, className);

            builder.CloseBlock(); // namespace

            return builder.Build();
        }

        /// <summary>
        /// Generates the NPC class definition with all members.
        /// </summary>
        private void GenerateNpcClass(ICodeBuilder builder, NpcBlueprint npc, string className)
        {
            // Class XML comment
            builder.AppendBlockComment(
                $"Auto-generated NPC blueprint for \"{CodeFormatter.EscapeString(npc.DisplayName)}\".",
                "Customize ConfigurePrefab and OnCreated to add unique logic."
            );

            builder.OpenBlock($"public sealed class {className} : NPC");

            // IsPhysical property
            builder.AppendLine($"public override bool IsPhysical => {npc.IsPhysical.ToString().ToLowerInvariant()};");

            // IsDealer property if applicable
            if (npc.IsDealer)
            {
                builder.AppendLine("public override bool IsDealer => true;");
            }

            builder.AppendLine();

            // ConfigurePrefab method
            GenerateConfigurePrefabMethod(builder, npc);

            // Constructor
            GenerateConstructor(builder, npc, className);

            // OnCreated method
            GenerateOnCreatedMethod(builder);

            builder.CloseBlock(); // class
        }

        /// <summary>
        /// Generates the ConfigurePrefab method with identity and appearance setup.
        /// </summary>
        private void GenerateConfigurePrefabMethod(ICodeBuilder builder, NpcBlueprint npc)
        {
            builder.OpenBlock("protected override void ConfigurePrefab(NPCPrefabBuilder builder)");

            // Identity
            builder.AppendLine($"builder.WithIdentity(\"{CodeFormatter.EscapeString(npc.NpcId)}\", \"{CodeFormatter.EscapeString(npc.FirstName)}\", \"{CodeFormatter.EscapeString(npc.LastName)}\")");

            // Appearance
            _appearanceGenerator.Generate(builder, npc.Appearance);

            // Spawn position
            if (npc.HasSpawnPosition)
            {
                builder.AppendLine($".WithSpawnPosition({CodeFormatter.FormatVector3(npc.SpawnX, npc.SpawnY, npc.SpawnZ)})");
            }

            // Customer configuration
            if (npc.EnableCustomer)
            {
                builder.AppendLine(".EnsureCustomer()");
                GenerateCustomerDefaults(builder, npc.CustomerDefaults);
            }

            // Dealer configuration
            if (npc.IsDealer)
            {
                builder.AppendLine(".EnsureDealer()");
                GenerateDealerDefaults(builder, npc.DealerDefaults);
            }

            // Relationship defaults
            if (ShouldGenerateRelationshipDefaults(npc.RelationshipDefaults))
            {
                GenerateRelationshipDefaults(builder, npc.RelationshipDefaults);
            }

            // Schedule
            if (npc.ScheduleActions != null && npc.ScheduleActions.Count > 0)
            {
                _scheduleGenerator.Generate(builder, npc);
            }

            // Inventory defaults
            if (ShouldGenerateInventoryDefaults(npc.InventoryDefaults))
            {
                GenerateInventoryDefaults(builder, npc.InventoryDefaults);
            }

            builder.AppendLine(";");

            builder.CloseBlock();
            builder.AppendLine();
        }

        private void GenerateCustomerDefaults(ICodeBuilder builder, NpcCustomerDefaults cd)
        {
            builder.OpenBlock(".WithCustomerDefaults(cd =>");
            builder.AppendLine($"cd.WithSpending({cd.MinWeeklySpending}f, {cd.MaxWeeklySpending}f)");
            builder.AppendLine($"  .WithOrdersPerWeek({cd.MinOrdersPerWeek}, {cd.MaxOrdersPerWeek})");
            builder.AppendLine($"  .WithPreferredOrderDay(Day.{cd.PreferredOrderDay})");
            builder.AppendLine($"  .WithOrderTime({cd.OrderTime})");
            builder.AppendLine($"  .WithStandards(CustomerStandard.{cd.CustomerStandards})");
            builder.AppendLine($"  .AllowDirectApproach({cd.AllowDirectApproach.ToString().ToLowerInvariant()})");

            // Advanced features
            if (cd.GuaranteeFirstSample)
            {
                builder.AppendLine($"  .GuaranteeFirstSample(true)");
            }

            if (cd.MutualRelationMinAt50 != 0 || cd.MutualRelationMaxAt100 != 0)
            {
                builder.AppendLine($"  .WithMutualRelationRequirement(minAt50: {cd.MutualRelationMinAt50}f, maxAt100: {cd.MutualRelationMaxAt100}f)");
            }

            if (cd.CallPoliceChance > 0)
            {
                builder.AppendLine($"  .WithCallPoliceChance({cd.CallPoliceChance}f)");
            }

            if (cd.BaseAddiction > 0 || cd.DependenceMultiplier != 1.0f)
            {
                builder.AppendLine($"  .WithDependence(baseAddiction: {cd.BaseAddiction}f, dependenceMultiplier: {cd.DependenceMultiplier}f)");
            }

            // Drug affinities
            if (cd.DrugAffinities != null && cd.DrugAffinities.Count > 0)
            {
                builder.AppendLine("  .WithAffinities(new[]");
                builder.AppendLine("  {");
                for (int i = 0; i < cd.DrugAffinities.Count; i++)
                {
                    var affinity = cd.DrugAffinities[i];
                    var comma = i < cd.DrugAffinities.Count - 1 ? "," : "";
                    builder.AppendLine($"      (DrugType.{affinity.DrugType}, {affinity.AffinityValue}f){comma}");
                }
                builder.AppendLine("  })");
            }

            // Preferred properties
            if (cd.PreferredProperties != null && cd.PreferredProperties.Count > 0)
            {
                var props = string.Join(", ", cd.PreferredProperties.Select(p => $"Property.{p}"));
                builder.AppendLine($"  .WithPreferredProperties({props})");
            }

            builder.AppendLine(";");
            builder.CloseBlock();
            builder.AppendLine(")");
        }

        private void GenerateDealerDefaults(ICodeBuilder builder, NpcDealerDefaults dd)
        {
            builder.OpenBlock(".WithDealerDefaults(dd =>");
            builder.AppendLine($"dd.WithSigningFee({dd.SigningFee}f)");
            builder.AppendLine($"  .WithCut({dd.CommissionCut}f)");
            builder.AppendLine($"  .WithDealerType(DealerType.{dd.DealerType})");

            if (!string.IsNullOrWhiteSpace(dd.HomeName))
            {
                builder.AppendLine($"  .WithHomeName(\"{CodeFormatter.EscapeString(dd.HomeName)}\")");
            }

            if (dd.AllowInsufficientQuality)
            {
                builder.AppendLine("  .AllowInsufficientQuality(true)");
            }

            if (!dd.AllowExcessQuality)
            {
                builder.AppendLine("  .AllowExcessQuality(false)");
            }

            if (!string.IsNullOrWhiteSpace(dd.CompletedDealsVariable))
            {
                builder.AppendLine($"  .WithCompletedDealsVariable(\"{CodeFormatter.EscapeString(dd.CompletedDealsVariable)}\")");
            }

            builder.AppendLine(";");
            builder.CloseBlock();
            builder.AppendLine(")");
        }

        private void GenerateRelationshipDefaults(ICodeBuilder builder, NpcRelationshipDefaults rd)
        {
            builder.OpenBlock(".WithRelationshipDefaults(r =>");

            if (rd.StartingDelta != 0)
            {
                builder.AppendLine($"r.WithDelta({rd.StartingDelta}f)");
            }
            else
            {
                builder.AppendLine("r.WithDelta(0f)");
            }

            if (rd.StartsUnlocked)
            {
                builder.AppendLine("  .SetUnlocked(true)");
            }
            else
            {
                builder.AppendLine("  .SetUnlocked(false)");
            }

            if (!string.IsNullOrWhiteSpace(rd.UnlockType))
            {
                builder.AppendLine($"  .SetUnlockType(NPCRelationship.UnlockType.{rd.UnlockType})");
            }

            if (rd.Connections != null && rd.Connections.Count > 0)
            {
                var connections = string.Join(", ", rd.Connections.Select(c => $"\"{CodeFormatter.EscapeString(c)}\""));
                builder.AppendLine($"  .WithConnectionsById({connections})");
            }

            builder.AppendLine(";");
            builder.CloseBlock();
            builder.AppendLine(")");
        }

        private void GenerateInventoryDefaults(ICodeBuilder builder, NpcInventoryDefaults inv)
        {
            builder.OpenBlock(".WithInventoryDefaults(inv =>");

            bool hasStartupItems = inv.StartupItems != null && inv.StartupItems.Count > 0;
            bool hasCash = inv.EnableRandomCash;
            bool hasClearNight = !inv.ClearInventoryEachNight;

            if (hasStartupItems)
            {
                var items = string.Join(", ", inv.StartupItems.Select(i => $"\"{CodeFormatter.EscapeString(i)}\""));
                builder.AppendLine($"inv.WithStartupItems({items})");
            }
            else
            {
                builder.AppendLine("inv");
            }

            if (hasCash)
            {
                builder.AppendLine($"   .WithRandomCash(min: {inv.RandomCashMin}f, max: {inv.RandomCashMax}f)");
            }

            if (hasClearNight)
            {
                builder.AppendLine("   .WithClearInventoryEachNight(false)");
            }

            builder.AppendLine(";");
            builder.CloseBlock();
            builder.AppendLine(")");
        }

        private bool ShouldGenerateRelationshipDefaults(NpcRelationshipDefaults rd)
        {
            return rd.StartingDelta != 0 ||
                   rd.StartsUnlocked ||
                   !string.IsNullOrWhiteSpace(rd.UnlockType) ||
                   (rd.Connections != null && rd.Connections.Count > 0);
        }

        private bool ShouldGenerateInventoryDefaults(NpcInventoryDefaults inv)
        {
            return inv.EnableRandomCash ||
                   !inv.ClearInventoryEachNight ||
                   (inv.StartupItems != null && inv.StartupItems.Count > 0);
        }

        /// <summary>
        /// Generates the NPC constructor.
        /// Identity is now configured via ConfigurePrefab using WithIdentity, not in the constructor.
        /// </summary>
        private void GenerateConstructor(ICodeBuilder builder, NpcBlueprint npc, string className)
        {
            builder.AppendLine($"public {className}() : base()");
            builder.OpenBlock();
            builder.CloseBlock();
            builder.AppendLine();
        }

        /// <summary>
        /// Generates the OnCreated method for NPC initialization.
        /// InitializeActions is no longer needed - actions are initialized automatically.
        /// </summary>
        private void GenerateOnCreatedMethod(ICodeBuilder builder)
        {
            builder.OpenBlock("protected override void OnCreated()");
            builder.AppendLine("base.OnCreated();");
            builder.AppendLine("Appearance.Build();");
            builder.AppendLine("Schedule.Enable();");
            builder.CloseBlock();
        }

        /// <summary>
        /// Validates whether the blueprint can be successfully generated.
        /// </summary>
        public CodeGenerationValidationResult Validate(NpcBlueprint blueprint)
        {
            var result = new CodeGenerationValidationResult { IsValid = true };

            if (blueprint == null)
            {
                result.IsValid = false;
                result.Errors.Add("Blueprint cannot be null");
                return result;
            }

            if (string.IsNullOrWhiteSpace(blueprint.ClassName))
            {
                result.Warnings.Add("Class name is empty, will use default 'GeneratedNpc'");
            }

            if (string.IsNullOrWhiteSpace(blueprint.NpcId))
            {
                result.Warnings.Add("NPC ID is empty");
            }

            if (string.IsNullOrWhiteSpace(blueprint.FirstName) && string.IsNullOrWhiteSpace(blueprint.LastName))
            {
                result.Warnings.Add("NPC has no name");
            }

            return result;
        }
    }
}
