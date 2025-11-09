using System;
using System.Collections.Generic;
using System.Reflection;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Configuration for S1API documentation URLs.
    /// </summary>
    public static class DocumentationConfig
    {
        /// <summary>
        /// Base URL for S1API documentation API pages.
        /// </summary>
        public const string BaseUrl = "https://ifbars.github.io/S1API/api";

        /// <summary>
        /// Constructs a full documentation URL for a given type name.
        /// </summary>
        /// <param name="fullTypeName">The full type name (e.g., "S1API.Entities.NPC")</param>
        /// <returns>The full URL to the documentation page</returns>
        public static string GetDocumentationUrl(string fullTypeName)
        {
            if (string.IsNullOrWhiteSpace(fullTypeName))
                return null;

            return $"{BaseUrl}/{fullTypeName}.html";
        }
    }

    /// <summary>
    /// Contains tooltip metadata including text and optional documentation URL.
    /// </summary>
    public class TooltipInfo
    {
        /// <summary>
        /// The tooltip text to display. Can be null or empty if no tooltip should be shown.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Optional URL to S1API documentation page. If provided, a "Learn More" link will be included in the tooltip.
        /// </summary>
        public string DocumentationUrl { get; set; }

        /// <summary>
        /// Whether this tooltip has any content to display.
        /// </summary>
        public bool HasContent => !string.IsNullOrWhiteSpace(Text) || !string.IsNullOrWhiteSpace(DocumentationUrl);

        public TooltipInfo(string text = null, string documentationUrl = null)
        {
            Text = text;
            DocumentationUrl = documentationUrl;
        }
    }

    /// <summary>
    /// Extracts tooltip information for WPF properties based on property names and types.
    /// </summary>
    public static class TooltipInfoExtractor
    {
        /// <summary>
        /// Gets tooltip information for a property based on its name and type.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyType">The type of the property (optional)</param>
        /// <param name="parentType">The type of the parent object (optional, for context)</param>
        /// <returns>TooltipInfo containing text and optional documentation URL</returns>
        public static TooltipInfo GetTooltipInfo(string propertyName, Type propertyType = null, Type parentType = null)
        {
            // Get description text from property name mapping
            var tooltipText = GetPropertyDescription(propertyName, parentType);

            // Map property type to S1API documentation URL
            string documentationUrl = null;
            if (propertyType != null)
            {
                documentationUrl = GetDocumentationUrlForType(propertyType);
            }

            return new TooltipInfo(tooltipText, documentationUrl);
        }

        /// <summary>
        /// Gets a description for a property based on common S1API property names.
        /// </summary>
        private static string GetPropertyDescription(string propertyName, Type parentType = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            // Map common property names to descriptions
            var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Quest properties
                { "ClassName", "The C# class name for this quest. Must be a valid C# identifier." },
                { "QuestId", "Unique identifier for this quest. Used for save/load and game systems." },
                { "QuestTitle", "Display name shown in UI elements and quest logs." },
                { "QuestDescription", "Description text displayed to the player when viewing quest details." },
                { "AutoBegin", "If enabled, the quest will start automatically when conditions are met." },
                { "CustomIcon", "If enabled, allows you to specify a custom icon resource for this quest." },
                { "QuestRewards", "If enabled, quest completion will grant rewards to the player." },
                
                // NPC properties
                { "NpcId", "Unique identifier used for save/load and game systems. Must be unique and descriptive (e.g., 'shopkeeper_alex')." },
                { "FirstName", "Display name shown in UI elements, dialogue, and messages." },
                { "LastName", "Optional last name. Combined with firstName for full name." },
                { "IsPhysical", "If true, the NPC will be visible in the game world with a 3D model, movement, and direct interaction. If false, the NPC is invisible and primarily used for messaging and phone interactions." },
                { "EnableCustomer", "If enabled, enables customer behavior systems for this NPC." },
                { "IsDealer", "If enabled, this NPC will function as a dealer NPC." },
                { "HasSpawnPosition", "If enabled, allows you to specify a custom spawn position for this NPC." },
                { "SpawnX", "X coordinate for NPC spawn position in world space." },
                { "SpawnY", "Y coordinate for NPC spawn position in world space." },
                { "SpawnZ", "Z coordinate for NPC spawn position in world space." },
                { "Namespace", "The C# namespace for this NPC class. Used for code generation." },
                
                // Appearance properties
                { "Gender", "Gender value (0-1). 0 typically represents male, 1 represents female." },
                { "Height", "NPC height value. Affects the NPC's overall scale." },
                { "Weight", "NPC weight value. Affects the NPC's body proportions." },
                { "PupilDilation", "Controls the dilation of the NPC's pupils." },
                { "EyebrowScale", "Scale factor for eyebrow size." },
                { "EyebrowThickness", "Thickness value for eyebrows." },
                { "HairPath", "Resource path to the hair style asset." },
                { "HairColor", "Color value for hair (hex format)." },
                { "SkinColor", "Color value for skin tone (hex format)." },
                
                // Trigger properties
                { "TargetAction", "The action identifier that triggers this quest trigger." },
                { "TargetNpcId", "The NPC ID associated with this trigger (for NPC-specific triggers)." },
                { "TriggerType", "The type of trigger (e.g., NPCEventTrigger, LocationTrigger, etc.)." },
                
                // Objective properties
                { "Title", "Display title for this quest objective." },
                { "RequiredProgress", "The amount of progress required to complete this objective." },
            };

            if (descriptions.TryGetValue(propertyName, out var description))
            {
                return description;
            }

            // Try to get description from parent type context
            if (parentType != null)
            {
                var fullPropertyName = $"{parentType.Name}.{propertyName}";
                if (descriptions.TryGetValue(fullPropertyName, out var contextualDescription))
                {
                    return contextualDescription;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the S1API documentation URL for a given type, if it's from the S1API namespace.
        /// </summary>
        /// <param name="type">The type to get documentation for</param>
        /// <returns>The documentation URL, or null if the type is not from S1API namespace</returns>
        private static string GetDocumentationUrlForType(Type type)
        {
            if (type == null)
                return null;

            // Check if type is from S1API namespace
            var fullTypeName = type.FullName;
            if (string.IsNullOrWhiteSpace(fullTypeName) || !fullTypeName.StartsWith("S1API."))
                return null;

            // Handle generic types - extract the base type name
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                fullTypeName = genericTypeDefinition.FullName;
                
                // Remove the backtick and number suffix (e.g., "List`1" -> "List")
                var backtickIndex = fullTypeName.IndexOf('`');
                if (backtickIndex >= 0)
                {
                    fullTypeName = fullTypeName.Substring(0, backtickIndex);
                }
            }

            // Handle nested types - they use '+' instead of '.'
            // DocFX uses '.' for nested types in URLs, so we need to convert
            fullTypeName = fullTypeName.Replace('+', '.');

            return DocumentationConfig.GetDocumentationUrl(fullTypeName);
        }
    }
}

