using Newtonsoft.Json;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents dealer behavior defaults for an NPC.
    /// </summary>
    public class NpcDealerDefaults : ObservableObject
    {
        private float _signingFee = 1000f;
        private float _commissionCut = 0.15f;
        private string _dealerType = "PlayerDealer";
        private string _homeName = string.Empty;
        private bool _allowInsufficientQuality;
        private bool _allowExcessQuality = true;
        private string _completedDealsVariable = string.Empty;

        [JsonProperty("signingFee")]
        public float SigningFee
        {
            get => _signingFee;
            set => SetProperty(ref _signingFee, value);
        }

        [JsonProperty("commissionCut")]
        public float CommissionCut
        {
            get => _commissionCut;
            set => SetProperty(ref _commissionCut, value);
        }

        [JsonProperty("dealerType")]
        public string DealerType
        {
            get => _dealerType;
            set => SetProperty(ref _dealerType, value ?? "PlayerDealer");
        }

        [JsonProperty("homeName")]
        public string HomeName
        {
            get => _homeName;
            set => SetProperty(ref _homeName, value ?? string.Empty);
        }

        [JsonProperty("allowInsufficientQuality")]
        public bool AllowInsufficientQuality
        {
            get => _allowInsufficientQuality;
            set => SetProperty(ref _allowInsufficientQuality, value);
        }

        [JsonProperty("allowExcessQuality")]
        public bool AllowExcessQuality
        {
            get => _allowExcessQuality;
            set => SetProperty(ref _allowExcessQuality, value);
        }

        [JsonProperty("completedDealsVariable")]
        public string CompletedDealsVariable
        {
            get => _completedDealsVariable;
            set => SetProperty(ref _completedDealsVariable, value ?? string.Empty);
        }

        public void CopyFrom(NpcDealerDefaults source)
        {
            if (source == null) return;

            SigningFee = source.SigningFee;
            CommissionCut = source.CommissionCut;
            DealerType = source.DealerType;
            HomeName = source.HomeName;
            AllowInsufficientQuality = source.AllowInsufficientQuality;
            AllowExcessQuality = source.AllowExcessQuality;
            CompletedDealsVariable = source.CompletedDealsVariable;
        }

        public NpcDealerDefaults DeepCopy()
        {
            return new NpcDealerDefaults
            {
                SigningFee = SigningFee,
                CommissionCut = CommissionCut,
                DealerType = DealerType,
                HomeName = HomeName,
                AllowInsufficientQuality = AllowInsufficientQuality,
                AllowExcessQuality = AllowExcessQuality,
                CompletedDealsVariable = CompletedDealsVariable
            };
        }
    }
}
