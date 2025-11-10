using System.Linq;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services.CodeGeneration.Abstractions;
using Schedule1ModdingTool.Services.CodeGeneration.Common;

namespace Schedule1ModdingTool.Services.CodeGeneration.Npc
{
    /// <summary>
    /// Generates schedule configuration code for NPCs.
    /// </summary>
    public class NpcScheduleGenerator
    {
        public void Generate(ICodeBuilder builder, NpcBlueprint npc)
        {
            if (npc.ScheduleActions == null || npc.ScheduleActions.Count == 0)
                return;

            builder.OpenBlock(".WithSchedule(plan =>");

            bool hasCustomerOrDealer = npc.EnableCustomer || npc.IsDealer;
            bool hasDealSignal = false;

            foreach (var action in npc.ScheduleActions)
            {
                switch (action.ActionType)
                {
                    case ScheduleActionType.EnsureDealSignal:
                        if (!hasDealSignal && hasCustomerOrDealer)
                        {
                            builder.AppendLine("plan.EnsureDealSignal()");
                            hasDealSignal = true;
                        }
                        break;

                    case ScheduleActionType.WalkTo:
                        GenerateWalkTo(builder, action);
                        break;

                    case ScheduleActionType.StayInBuilding:
                        GenerateStayInBuilding(builder, action);
                        break;

                    case ScheduleActionType.LocationDialogue:
                        GenerateLocationDialogue(builder, action);
                        break;

                    case ScheduleActionType.UseVendingMachine:
                        builder.AppendLine($"    .UseVendingMachine({action.StartTime})");
                        break;

                    case ScheduleActionType.DriveToCarPark:
                        GenerateDriveToCarPark(builder, action);
                        break;

                    case ScheduleActionType.UseATM:
                        builder.AppendLine($"    .UseATM({action.StartTime})");
                        break;

                    case ScheduleActionType.HandleDeal:
                        if (npc.IsDealer)
                        {
                            builder.AppendLine($"    .HandleDeal({action.StartTime})");
                        }
                        break;
                }
            }

            builder.AppendLine(";");
            builder.CloseBlock();
            builder.AppendLine(")");
        }

        private void GenerateWalkTo(ICodeBuilder builder, NpcScheduleAction action)
        {
            var pos = CodeFormatter.FormatVector3(action.PositionX, action.PositionY, action.PositionZ);

            if (action.FaceDestinationDirection)
            {
                builder.AppendLine($"    .WalkTo({pos}, {action.StartTime}, faceDestinationDir: true)");
            }
            else
            {
                builder.AppendLine($"    .WalkTo({pos}, {action.StartTime})");
            }
        }

        private void GenerateStayInBuilding(ICodeBuilder builder, NpcScheduleAction action)
        {
            if (string.IsNullOrWhiteSpace(action.BuildingName))
            {
                // If no building specified, generate a comment
                builder.AppendLine($"    // .StayInBuilding(building, {action.StartTime}, {action.Duration})");
                return;
            }

            var safeBuilding = CodeFormatter.EscapeString(action.BuildingName);
            if (action.Duration > 0)
            {
                builder.AppendLine($"    .StayInBuilding(Building.Get<{safeBuilding}>(), {action.StartTime}, {action.Duration})");
            }
            else
            {
                builder.AppendLine($"    .StayInBuilding(Building.Get<{safeBuilding}>(), {action.StartTime})");
            }
        }

        private void GenerateLocationDialogue(ICodeBuilder builder, NpcScheduleAction action)
        {
            var pos = CodeFormatter.FormatVector3(action.PositionX, action.PositionY, action.PositionZ);
            builder.AppendLine($"    .LocationDialogue({pos}, {action.StartTime})");
        }

        private void GenerateDriveToCarPark(ICodeBuilder builder, NpcScheduleAction action)
        {
            if (string.IsNullOrWhiteSpace(action.ParkingLotName) || string.IsNullOrWhiteSpace(action.VehicleId))
            {
                // If incomplete, generate a comment
                builder.AppendLine($"    // .DriveToCarParkWithCreateVehicle(parkingLot, vehicleId, {action.StartTime}, spawnPos, rotation, ParkingAlignment.{action.ParkingAlignment})");
                return;
            }

            var safeParkingLot = CodeFormatter.EscapeString(action.ParkingLotName);
            var safeVehicle = CodeFormatter.EscapeString(action.VehicleId);
            var spawnPos = CodeFormatter.FormatVector3(action.VehicleSpawnX, action.VehicleSpawnY, action.VehicleSpawnZ);
            var rotation = $"Quaternion.Euler({action.VehicleRotationX}f, {action.VehicleRotationY}f, {action.VehicleRotationZ}f)";

            builder.AppendLine($"    .DriveToCarParkWithCreateVehicle(\"{safeParkingLot}\", \"{safeVehicle}\",");
            builder.AppendLine($"        {action.StartTime}, {spawnPos}, {rotation}, ParkingAlignment.{action.ParkingAlignment})");
        }
    }
}
