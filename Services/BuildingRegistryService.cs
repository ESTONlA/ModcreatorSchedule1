using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Schedule1ModdingTool.ViewModels;

namespace Schedule1ModdingTool.Services
{
    /// <summary>
    /// Service that provides a catalog of available building identifier types from S1API.Map.Buildings.
    /// </summary>
    public static class BuildingRegistryService
    {
        private static List<BuildingInfo>? _cachedBuildings;

        /// <summary>
        /// Gets all available building identifier types.
        /// </summary>
        public static List<BuildingInfo> GetAllBuildings()
        {
            if (_cachedBuildings != null)
                return _cachedBuildings;

            var buildings = new List<BuildingInfo>();

            // Hardcoded list of all building identifier types from S1API.Map.Buildings namespace
            // These match the typed identifiers available in S1API
            var buildingTypes = new[]
            {
                ("ApartmentBuilding", "Apartment Buiding"),
                ("Arcade", "Arcade"),
                ("BenjisMotelRoom", "Benji's Motel Room"),
                ("BethsRoom", "Beth's Room"),
                ("BoutiqueStore", "Boutique Store"),
                ("BradsTent", "Brad's Tent"),
                ("BudsBar", "Bud's Bar"),
                ("Cafe", "Cafe"),
                ("Caravan", "Caravan"),
                ("CarlsHouse", "Carl's House"),
                ("Casino", "Casino"),
                ("CharlesHouse", "Charles' House"),
                ("ChemicalPlantA", "Chemical Plant A"),
                ("ChineseRestaurant", "Chinese Restaurant"),
                ("Church", "Church"),
                ("CommunityCenter", "Community Center"),
                ("CornerStore", "Corner Store"),
                ("Courthouse", "Courthouse"),
                ("DansHardwareUpstairs", "Dan's Hardware Upstairs"),
                ("DocksIndustrialBuilding", "Docks Industrial Building"),
                ("DocksShippingContainer", "Docks Shipping Container"),
                ("FireStation", "Fire Station"),
                ("FishWarehouse", "Fish Warehouse"),
                ("GeorgeAndMollysHouse", "George and Molly's House"),
                ("GoblinHideBuilding", "Goblin Hide Building"),
                ("HAMLegal", "HAM Legal"),
                ("HoltHouse", "Holt House"),
                ("HylandBank", "Hyland Bank"),
                ("HylandMedical", "Hyland Medical"),
                ("JanesCaravan", "Jane's Caravan"),
                ("JerrysTent", "Jerry's Tent"),
                ("JessisRoom", "Jessi's Room"),
                ("KennedyHouse", "Kennedy House"),
                ("KnightHouse", "Knight House"),
                ("KyleAndAustinsHouse", "Kyle and Austin's House"),
                ("LeosShippingContainer", "Leo's Shipping Container"),
                ("LesOrduresPuantes", "Les Ordures Puantes"),
                ("MayorsHouse", "Mayor's House"),
                ("MicksHouse", "Mick's House"),
                ("ModernMansion", "Modern Mansion"),
                ("MotelOffice", "Motel Office"),
                ("Nightclub", "Nightclub"),
                ("NorthApartments", "North apartments"),
                ("NorthIndustrialBuilding", "North Industrial Building"),
                ("NorthWarehouse", "North Warehouse"),
                ("OverpassBuilding", "Overpass Building"),
                ("PawnShop", "Pawn Shop"),
                ("PetersRoom", "Peter's Room"),
                ("Pillville", "Pillville"),
                ("PoliceStation", "Police Station"),
                ("RandysBaitTackle", "Randy's Bait & Tackle"),
                ("Room1", "Room 1"),
                ("Room2", "Room 2"),
                ("Room3", "Room 3"),
                ("SauerkrautSupreme", "Sauerkraut Supreme"),
                ("Shack", "Shack"),
                ("ShermanHouse", "Sherman House"),
                ("ShootingRange", "Shooting Range"),
                ("SlopShop", "Slop Shop"),
                ("SouthOverpassBuilding", "South Overpass Building"),
                ("StevensonHouse", "Stevenson House"),
                ("StorageUnit2", "Storage Unit 2"),
                ("StorageUnit3", "Storage Unit 3"),
                ("Supermarket", "Supermarket"),
                ("TallTower", "Tall Tower"),
                ("TheCrimsonCanary", "The Crimson Canary"),
                ("ThePissHut", "The Piss Hut"),
                ("ThompsonConstructionDemo", "Thompson Construction Demo"),
                ("ThompsonHouse", "Thompson House"),
                ("TownHall", "Town Hall"),
                ("UpscaleApartments", "Upscale Apartments"),
                ("WebsterHouse", "Webster House"),
                ("WeisCabin", "Weis Cabin"),
                ("WestGasMart", "West Gas Mart"),
                ("WilkinsonHouse", "Wilkinson House")
            };

            foreach (var (typeName, displayName) in buildingTypes.OrderBy(b => b.Item2))
            {
                buildings.Add(new BuildingInfo
                {
                    TypeName = typeName,
                    DisplayName = displayName,
                    FullTypeName = $"S1API.Map.Buildings.{typeName}"
                });
            }

            _cachedBuildings = buildings;
            return buildings;
        }

        /// <summary>
        /// Gets a building info by type name.
        /// </summary>
        public static BuildingInfo? GetBuildingByTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            return GetAllBuildings().FirstOrDefault(b => 
                b.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase) ||
                b.FullTypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a building info by display name.
        /// </summary>
        public static BuildingInfo? GetBuildingByDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return null;

            return GetAllBuildings().FirstOrDefault(b => 
                b.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
        }
    }
}

