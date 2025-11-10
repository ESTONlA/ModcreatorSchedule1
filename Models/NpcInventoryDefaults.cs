using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents default inventory settings for an NPC.
    /// </summary>
    public class NpcInventoryDefaults : ObservableObject
    {
        private bool _enableRandomCash;
        private float _randomCashMin = 50f;
        private float _randomCashMax = 500f;
        private bool _clearInventoryEachNight = true;

        [JsonProperty("enableRandomCash")]
        public bool EnableRandomCash
        {
            get => _enableRandomCash;
            set => SetProperty(ref _enableRandomCash, value);
        }

        [JsonProperty("randomCashMin")]
        public float RandomCashMin
        {
            get => _randomCashMin;
            set => SetProperty(ref _randomCashMin, value);
        }

        [JsonProperty("randomCashMax")]
        public float RandomCashMax
        {
            get => _randomCashMax;
            set => SetProperty(ref _randomCashMax, value);
        }

        [JsonProperty("clearInventoryEachNight")]
        public bool ClearInventoryEachNight
        {
            get => _clearInventoryEachNight;
            set => SetProperty(ref _clearInventoryEachNight, value);
        }

        [JsonProperty("startupItems")]
        public ObservableCollection<string> StartupItems { get; } = new();

        public void CopyFrom(NpcInventoryDefaults source)
        {
            if (source == null) return;

            EnableRandomCash = source.EnableRandomCash;
            RandomCashMin = source.RandomCashMin;
            RandomCashMax = source.RandomCashMax;
            ClearInventoryEachNight = source.ClearInventoryEachNight;

            StartupItems.Clear();
            foreach (var item in source.StartupItems)
            {
                StartupItems.Add(item);
            }
        }

        public NpcInventoryDefaults DeepCopy()
        {
            var copy = new NpcInventoryDefaults
            {
                EnableRandomCash = EnableRandomCash,
                RandomCashMin = RandomCashMin,
                RandomCashMax = RandomCashMax,
                ClearInventoryEachNight = ClearInventoryEachNight
            };

            foreach (var item in StartupItems)
            {
                copy.StartupItems.Add(item);
            }

            return copy;
        }
    }
}
