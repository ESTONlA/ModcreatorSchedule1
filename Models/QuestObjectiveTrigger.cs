using Newtonsoft.Json;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents a trigger specifically for quest objectives
    /// Extends QuestTrigger with objective-specific context
    /// </summary>
    public class QuestObjectiveTrigger : QuestTrigger
    {
        private string _objectiveName = "";

        /// <summary>
        /// The name of the objective this trigger targets (alternative to ObjectiveIndex)
        /// </summary>
        [JsonProperty("objectiveName")]
        public string ObjectiveName
        {
            get => _objectiveName;
            set => SetProperty(ref _objectiveName, value);
        }

        public QuestObjectiveTrigger() : base()
        {
        }

        public QuestObjectiveTrigger(QuestTriggerType triggerType, string targetAction, QuestTriggerTarget triggerTarget, string objectiveName)
            : base(triggerType, targetAction, triggerTarget)
        {
            ObjectiveName = objectiveName;
        }

        public new QuestObjectiveTrigger DeepCopy()
        {
            return new QuestObjectiveTrigger
            {
                TriggerType = TriggerType,
                TargetAction = TargetAction,
                TargetNpcId = TargetNpcId,
                TriggerTarget = TriggerTarget,
                ObjectiveIndex = ObjectiveIndex,
                ObjectiveName = ObjectiveName
            };
        }
    }
}

