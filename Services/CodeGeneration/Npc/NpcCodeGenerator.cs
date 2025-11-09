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

        public NpcCodeGenerator()
        {
            _headerGenerator = new NpcHeaderGenerator();
            _appearanceGenerator = new NpcAppearanceGenerator();
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
                builder.OpenBlock(".WithCustomerDefaults(cd =>");
                builder.AppendLine("cd.WithSpending(400f, 900f)");
                builder.AppendLine("  .WithOrdersPerWeek(1, 3)");
                builder.AppendLine("  .WithPreferredOrderDay(Day.Monday)");
                builder.AppendLine("  .WithOrderTime(900)");
                builder.AppendLine("  .WithStandards(CustomerStandard.Moderate)");
                builder.AppendLine("  .AllowDirectApproach(true);");
                builder.CloseBlock();
                builder.AppendLine(")");
            }

            // Dealer configuration
            if (npc.IsDealer)
            {
                builder.AppendLine(".EnsureDealer()");
                builder.OpenBlock(".WithDealerDefaults(dd =>");
                builder.AppendLine("dd.WithSigningFee(1000f)");
                builder.AppendLine("  .WithCut(0.15f)");
                builder.AppendLine("  .WithDealerType(DealerType.PlayerDealer);");
                builder.CloseBlock();
                builder.AppendLine(")");
            }

            builder.AppendLine(";");

            builder.CloseBlock();
            builder.AppendLine();
        }

        /// <summary>
        /// Generates the NPC constructor.
        /// </summary>
        private void GenerateConstructor(ICodeBuilder builder, NpcBlueprint npc, string className)
        {
            builder.AppendLine($"public {className}() : base(");
            builder.IncreaseIndent();
            builder.AppendLine($"id: \"{CodeFormatter.EscapeString(npc.NpcId)}\",");
            builder.AppendLine($"firstName: \"{CodeFormatter.EscapeString(npc.FirstName)}\",");
            builder.AppendLine($"lastName: \"{CodeFormatter.EscapeString(npc.LastName)}\",");
            builder.AppendLine("icon: null)");
            builder.DecreaseIndent();
            builder.OpenBlock();
            builder.CloseBlock();
            builder.AppendLine();
        }

        /// <summary>
        /// Generates the OnCreated method for NPC initialization.
        /// </summary>
        private void GenerateOnCreatedMethod(ICodeBuilder builder)
        {
            builder.OpenBlock("protected override void OnCreated()");
            builder.AppendLine("base.OnCreated();");
            builder.AppendLine("Appearance.Build();");
            builder.AppendLine("Schedule.Enable();");
            builder.AppendLine("Schedule.InitializeActions();");
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
