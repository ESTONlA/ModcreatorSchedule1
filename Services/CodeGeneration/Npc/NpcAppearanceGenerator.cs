using System;
using System.Linq;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services.CodeGeneration.Abstractions;
using Schedule1ModdingTool.Services.CodeGeneration.Common;

namespace Schedule1ModdingTool.Services.CodeGeneration.Npc
{
    /// <summary>
    /// Generates NPC appearance builder code from appearance presets.
    /// Handles appearance properties, face/body/accessory layers, and color settings.
    /// </summary>
    public class NpcAppearanceGenerator
    {
        /// <summary>
        /// Generates the appearance builder code within ConfigurePrefab.
        /// </summary>
        public void Generate(ICodeBuilder builder, NpcAppearanceSettings? appearance)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (appearance == null)
            {
                builder.AppendLine(".WithAppearanceDefaults(av => { })");
                return;
            }

            builder.OpenBlock(".WithAppearanceDefaults(av =>");

            // Basic appearance properties
            builder.AppendLine($"av.Gender = {CodeFormatter.FormatFloat(appearance.Gender)}f;");
            builder.AppendLine($"av.Height = {CodeFormatter.FormatFloat(appearance.Height)}f;");
            builder.AppendLine($"av.Weight = {CodeFormatter.FormatFloat(appearance.Weight)}f;");
            builder.AppendLine($"av.SkinColor = {CodeFormatter.FormatColor32FromHex(appearance.SkinColor)};");

            // Eye properties
            builder.AppendLine($"av.LeftEyeLidColor = {CodeFormatter.FormatColorFromHex(appearance.LeftEyeLidColor)};");
            builder.AppendLine($"av.RightEyeLidColor = {CodeFormatter.FormatColorFromHex(appearance.RightEyeLidColor)};");
            builder.AppendLine($"av.EyeBallTint = {CodeFormatter.FormatColorFromHex(appearance.EyeBallTint)};");

            // Hair properties
            builder.AppendLine($"av.HairColor = {CodeFormatter.FormatColorFromHex(appearance.HairColor)};");
            builder.AppendLine($"av.HairPath = \"{CodeFormatter.EscapeString(appearance.HairPath)}\";");

            // Eyeball material and pupil
            builder.AppendLine($"av.EyeballMaterialIdentifier = \"{CodeFormatter.EscapeString(appearance.EyeballMaterialIdentifier)}\";");
            builder.AppendLine($"av.PupilDilation = {CodeFormatter.FormatFloat(appearance.PupilDilation)}f;");

            // Eyebrow properties
            builder.AppendLine($"av.EyebrowScale = {CodeFormatter.FormatFloat(appearance.EyebrowScale)}f;");
            builder.AppendLine($"av.EyebrowThickness = {CodeFormatter.FormatFloat(appearance.EyebrowThickness)}f;");
            builder.AppendLine($"av.EyebrowRestingHeight = {CodeFormatter.FormatFloat(appearance.EyebrowRestingHeight)}f;");
            builder.AppendLine($"av.EyebrowRestingAngle = {CodeFormatter.FormatFloat(appearance.EyebrowRestingAngle)}f;");

            // Eye tuples
            builder.AppendLine($"av.LeftEye = {CodeFormatter.FormatTuple((float)appearance.LeftEyeTop, (float)appearance.LeftEyeBottom)};");
            builder.AppendLine($"av.RightEye = {CodeFormatter.FormatTuple((float)appearance.RightEyeTop, (float)appearance.RightEyeBottom)};");

            // Face layers
            foreach (var layer in appearance.FaceLayers)
            {
                builder.AppendLine($"av.WithFaceLayer(\"{CodeFormatter.EscapeString(layer.LayerPath)}\", {CodeFormatter.FormatColorFromHex(layer.ColorHex)});");
            }

            // Body layers
            foreach (var layer in appearance.BodyLayers)
            {
                builder.AppendLine($"av.WithBodyLayer(\"{CodeFormatter.EscapeString(layer.LayerPath)}\", {CodeFormatter.FormatColorFromHex(layer.ColorHex)});");
            }

            // Accessory layers
            foreach (var layer in appearance.AccessoryLayers)
            {
                builder.AppendLine($"av.WithAccessoryLayer(\"{CodeFormatter.EscapeString(layer.LayerPath)}\", {CodeFormatter.FormatColorFromHex(layer.ColorHex)});");
            }

            builder.CloseBlock();
            builder.AppendLine(")");
        }
    }
}
