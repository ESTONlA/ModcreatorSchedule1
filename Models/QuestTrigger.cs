using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Schedule1ModdingTool.Services;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents a trigger that can activate quest lifecycle events
    /// </summary>
    public class QuestTrigger : ObservableObject
    {
        private QuestTriggerType _triggerType = QuestTriggerType.ActionTrigger;
        private string _targetAction = "";
        private string _targetNpcId = "";
        private QuestTriggerTarget _triggerTarget = QuestTriggerTarget.QuestStart;
        private int? _objectiveIndex = null;
        private TriggerMetadata _selectedTriggerMetadata;

        /// <summary>
        /// The type of trigger (Action, NPC Event, or Custom)
        /// </summary>
        [JsonProperty("triggerType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public QuestTriggerType TriggerType
        {
            get => _triggerType;
            set => SetProperty(ref _triggerType, value);
        }

        /// <summary>
        /// The target action to subscribe to (e.g., "TimeManager.OnDayPass", "NPC.OnDeath")
        /// Format: "ClassName.ActionName"
        /// </summary>
        [JsonProperty("targetAction")]
        public string TargetAction
        {
            get => _targetAction;
            set => SetProperty(ref _targetAction, value);
        }

        /// <summary>
        /// The NPC ID for NPC-specific triggers (required when TriggerType is NPCEventTrigger)
        /// </summary>
        [JsonProperty("targetNpcId")]
        public string TargetNpcId
        {
            get => _targetNpcId;
            set => SetProperty(ref _targetNpcId, value);
        }

        /// <summary>
        /// What this trigger should activate (Quest Start, Quest Finish, Objective Start, Objective Finish)
        /// </summary>
        [JsonProperty("triggerTarget")]
        [JsonConverter(typeof(StringEnumConverter))]
        public QuestTriggerTarget TriggerTarget
        {
            get => _triggerTarget;
            set => SetProperty(ref _triggerTarget, value);
        }

        /// <summary>
        /// The index of the objective this trigger targets (required when TriggerTarget is ObjectiveStart or ObjectiveFinish)
        /// </summary>
        [JsonProperty("objectiveIndex")]
        public int? ObjectiveIndex
        {
            get => _objectiveIndex;
            set => SetProperty(ref _objectiveIndex, value);
        }

        /// <summary>
        /// The selected trigger metadata (for UI binding, not serialized)
        /// </summary>
        [JsonIgnore]
        public TriggerMetadata SelectedTriggerMetadata
        {
            get => _selectedTriggerMetadata;
            set
            {
                if (SetProperty(ref _selectedTriggerMetadata, value) && value != null)
                {
                    TargetAction = value.TargetAction;
                    // Only update TriggerType if it's different - this preserves the saved TriggerType
                    // when syncing metadata after loading from JSON
                    if (TriggerType != value.TriggerType)
                    {
                        TriggerType = value.TriggerType;
                    }
                }
            }
        }

        public QuestTrigger()
        {
        }

        public QuestTrigger(QuestTriggerType triggerType, string targetAction, QuestTriggerTarget triggerTarget)
        {
            TriggerType = triggerType;
            TargetAction = targetAction;
            TriggerTarget = triggerTarget;
        }

        public QuestTrigger DeepCopy()
        {
            return new QuestTrigger
            {
                TriggerType = TriggerType,
                TargetAction = TargetAction,
                TargetNpcId = TargetNpcId,
                TriggerTarget = TriggerTarget,
                ObjectiveIndex = ObjectiveIndex
            };
        }

        public override string ToString()
        {
            var npcPart = !string.IsNullOrWhiteSpace(TargetNpcId) ? $" (NPC: {TargetNpcId})" : "";
            var objPart = ObjectiveIndex.HasValue ? $" (Objective: {ObjectiveIndex})" : "";
            return $"{TriggerTarget}: {TargetAction}{npcPart}{objPart}";
        }
    }

    /// <summary>
    /// Types of triggers available
    /// </summary>
    public enum QuestTriggerType
    {
        /// <summary>
        /// Static Action trigger from S1API (e.g., TimeManager.OnDayPass)
        /// </summary>
        ActionTrigger,

        /// <summary>
        /// NPC instance event trigger (e.g., NPC.OnDeath for a specific NPC)
        /// </summary>
        NPCEventTrigger,

        /// <summary>
        /// Custom trigger type for future extensibility
        /// </summary>
        CustomTrigger
    }

    /// <summary>
    /// What the trigger should activate
    /// </summary>
    public enum QuestTriggerTarget
    {
        /// <summary>
        /// Start the quest
        /// </summary>
        QuestStart,

        /// <summary>
        /// Finish/complete the quest
        /// </summary>
        QuestFinish,

        /// <summary>
        /// Start a specific objective
        /// </summary>
        ObjectiveStart,

        /// <summary>
        /// Complete a specific objective
        /// </summary>
        ObjectiveFinish
    }
}

