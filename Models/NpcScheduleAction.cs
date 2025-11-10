using System.ComponentModel;
using Newtonsoft.Json;
using Schedule1ModdingTool.Utils;

namespace Schedule1ModdingTool.Models
{
    /// <summary>
    /// Represents a single action in an NPC's daily schedule.
    /// </summary>
    public class NpcScheduleAction : ObservableObject
    {
        private ScheduleActionType _actionType = ScheduleActionType.WalkTo;
        private int _startTime = 900; // 9:00 AM
        private int _duration = 60; // minutes
        private float _positionX;
        private float _positionY;
        private float _positionZ;
        private string _buildingName = string.Empty;
        private string _parkingLotName = string.Empty;
        private string _vehicleId = string.Empty;
        private bool _faceDestinationDirection;
        private string _parkingAlignment = "FrontToKerb";
        private float _vehicleSpawnX;
        private float _vehicleSpawnY;
        private float _vehicleSpawnZ;
        private float _vehicleRotationX;
        private float _vehicleRotationY;
        private float _vehicleRotationZ;

        [JsonProperty("actionType")]
        public ScheduleActionType ActionType
        {
            get => _actionType;
            set
            {
                if (SetProperty(ref _actionType, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        [JsonProperty("startTime")]
        public int StartTime
        {
            get => _startTime;
            set
            {
                if (SetProperty(ref _startTime, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        [JsonProperty("duration")]
        public int Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        [JsonProperty("positionX")]
        public float PositionX
        {
            get => _positionX;
            set => SetProperty(ref _positionX, value);
        }

        [JsonProperty("positionY")]
        public float PositionY
        {
            get => _positionY;
            set => SetProperty(ref _positionY, value);
        }

        [JsonProperty("positionZ")]
        public float PositionZ
        {
            get => _positionZ;
            set => SetProperty(ref _positionZ, value);
        }

        [JsonProperty("buildingName")]
        public string BuildingName
        {
            get => _buildingName;
            set => SetProperty(ref _buildingName, value ?? string.Empty);
        }

        [JsonProperty("parkingLotName")]
        public string ParkingLotName
        {
            get => _parkingLotName;
            set => SetProperty(ref _parkingLotName, value ?? string.Empty);
        }

        [JsonProperty("vehicleId")]
        public string VehicleId
        {
            get => _vehicleId;
            set => SetProperty(ref _vehicleId, value ?? string.Empty);
        }

        [JsonProperty("faceDestinationDirection")]
        public bool FaceDestinationDirection
        {
            get => _faceDestinationDirection;
            set => SetProperty(ref _faceDestinationDirection, value);
        }

        [JsonProperty("parkingAlignment")]
        public string ParkingAlignment
        {
            get => _parkingAlignment;
            set => SetProperty(ref _parkingAlignment, value ?? "FrontToKerb");
        }

        [JsonProperty("vehicleSpawnX")]
        public float VehicleSpawnX
        {
            get => _vehicleSpawnX;
            set => SetProperty(ref _vehicleSpawnX, value);
        }

        [JsonProperty("vehicleSpawnY")]
        public float VehicleSpawnY
        {
            get => _vehicleSpawnY;
            set => SetProperty(ref _vehicleSpawnY, value);
        }

        [JsonProperty("vehicleSpawnZ")]
        public float VehicleSpawnZ
        {
            get => _vehicleSpawnZ;
            set => SetProperty(ref _vehicleSpawnZ, value);
        }

        [JsonProperty("vehicleRotationX")]
        public float VehicleRotationX
        {
            get => _vehicleRotationX;
            set => SetProperty(ref _vehicleRotationX, value);
        }

        [JsonProperty("vehicleRotationY")]
        public float VehicleRotationY
        {
            get => _vehicleRotationY;
            set => SetProperty(ref _vehicleRotationY, value);
        }

        [JsonProperty("vehicleRotationZ")]
        public float VehicleRotationZ
        {
            get => _vehicleRotationZ;
            set => SetProperty(ref _vehicleRotationZ, value);
        }

        [JsonIgnore]
        public string DisplayName => $"{StartTime:D4} - {ActionType}";

        public NpcScheduleAction DeepCopy()
        {
            return new NpcScheduleAction
            {
                ActionType = ActionType,
                StartTime = StartTime,
                Duration = Duration,
                PositionX = PositionX,
                PositionY = PositionY,
                PositionZ = PositionZ,
                BuildingName = BuildingName,
                ParkingLotName = ParkingLotName,
                VehicleId = VehicleId,
                FaceDestinationDirection = FaceDestinationDirection,
                ParkingAlignment = ParkingAlignment,
                VehicleSpawnX = VehicleSpawnX,
                VehicleSpawnY = VehicleSpawnY,
                VehicleSpawnZ = VehicleSpawnZ,
                VehicleRotationX = VehicleRotationX,
                VehicleRotationY = VehicleRotationY,
                VehicleRotationZ = VehicleRotationZ
            };
        }
    }

    /// <summary>
    /// Types of schedule actions supported by S1API.
    /// </summary>
    public enum ScheduleActionType
    {
        [Description("Walk to Location")]
        WalkTo,

        [Description("Stay in Building")]
        StayInBuilding,

        [Description("Location Dialogue")]
        LocationDialogue,

        [Description("Use Vending Machine")]
        UseVendingMachine,

        [Description("Drive to Car Park")]
        DriveToCarPark,

        [Description("Use ATM")]
        UseATM,

        [Description("Handle Deal (Dealer Only)")]
        HandleDeal,

        [Description("Ensure Deal Signal (Customer Only)")]
        EnsureDealSignal
    }
}
