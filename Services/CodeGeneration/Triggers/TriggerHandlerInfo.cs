using System;
using Schedule1ModdingTool.Models;

namespace Schedule1ModdingTool.Services.CodeGeneration.Triggers
{
    /// <summary>
    /// Contains metadata about a trigger handler that needs to be generated.
    /// Used to coordinate trigger field generation and subscription code.
    /// </summary>
    public class TriggerHandlerInfo
    {
        /// <summary>
        /// The trigger this handler is for (either QuestTrigger or QuestObjectiveTrigger).
        /// </summary>
        public QuestTrigger? Trigger { get; set; }

        /// <summary>
        /// The field name for the Action handler (e.g., "_onDeathHandler").
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The method to call when triggered (e.g., "Begin()", "Complete()", "Fail()").
        /// </summary>
        public string ActionMethod { get; set; } = string.Empty;

        /// <summary>
        /// The index of the objective this trigger affects (null for quest-level triggers).
        /// </summary>
        public int? ObjectiveIndex { get; set; }

        /// <summary>
        /// The category of trigger for organizational purposes.
        /// </summary>
        public TriggerCategory TriggerCategory { get; set; }
    }

    /// <summary>
    /// Categorizes triggers by their purpose.
    /// </summary>
    public enum TriggerCategory
    {
        /// <summary>
        /// Triggers that start the quest.
        /// </summary>
        QuestStart,

        /// <summary>
        /// Triggers that complete/fail/cancel the quest.
        /// </summary>
        QuestFinish,

        /// <summary>
        /// Triggers that activate an objective.
        /// </summary>
        ObjectiveStart,

        /// <summary>
        /// Triggers that complete an objective.
        /// </summary>
        ObjectiveFinish
    }
}
