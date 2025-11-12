using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents default relationship settings for an NPC.
    /// </summary>
    public class NpcRelationshipDefaults : ObservableObject
    {
        private float _startingDelta = 0f;
        private bool _startsUnlocked;
        private string _unlockType = "DirectApproach";

        [JsonProperty("startingDelta")]
        public float StartingDelta
        {
            get => _startingDelta;
            set => SetProperty(ref _startingDelta, value);
        }

        [JsonProperty("startsUnlocked")]
        public bool StartsUnlocked
        {
            get => _startsUnlocked;
            set => SetProperty(ref _startsUnlocked, value);
        }

        [JsonProperty("unlockType")]
        public string UnlockType
        {
            get => _unlockType;
            set => SetProperty(ref _unlockType, value ?? "DirectApproach");
        }

        [JsonProperty("connections")]
        public ObservableCollection<string> Connections { get; } = new();

        public void CopyFrom(NpcRelationshipDefaults source)
        {
            if (source == null) return;

            StartingDelta = source.StartingDelta;
            StartsUnlocked = source.StartsUnlocked;
            UnlockType = source.UnlockType;

            Connections.Clear();
            foreach (var connection in source.Connections)
            {
                Connections.Add(connection);
            }
        }

        public NpcRelationshipDefaults DeepCopy()
        {
            var copy = new NpcRelationshipDefaults
            {
                StartingDelta = StartingDelta,
                StartsUnlocked = StartsUnlocked,
                UnlockType = UnlockType
            };

            foreach (var connection in Connections)
            {
                copy.Connections.Add(connection);
            }

            return copy;
        }
    }
}
