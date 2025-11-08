using System;
using System.Collections.Generic;

namespace Schedule1ModdingTool.Data
{
    public sealed class AppearancePresetOption
    {
        public AppearancePresetOption(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; }
        public string Path { get; }

        public override string ToString() => Path;
    }

    public static class AppearancePresets
    {
        public static IReadOnlyList<AppearancePresetOption> HairStyles { get; } = Create(
            Option("Afro", "Avatar/Hair/afro/Afro"),
            Option("Balding", "Avatar/Hair/balding/Balding"),
            Option("Bowl Cut", "Avatar/Hair/bowlcut/BowlCut"),
            Option("Bun", "Avatar/Hair/bun/Bun"),
            Option("Buzz Cut", "Avatar/Hair/buzzcut/BuzzCut"),
            Option("Double Top Knot", "Avatar/Hair/doubletopknot/DoubleTopKnot"),
            Option("Franklin", "Avatar/Hair/franklin/Franklin"),
            Option("Fringe Pony Tail", "Avatar/Hair/fringeponytail/FringePonyTail"),
            Option("High Bun", "Avatar/Hair/highbun/HighBun"),
            Option("Jesus", "Avatar/Hair/jesus/Jesus"),
            Option("Long Curly", "Avatar/Hair/longcurly/LongCurly"),
            Option("Low Bun", "Avatar/Hair/lowbun/LowBun"),
            Option("Messy Bob", "Avatar/Hair/messybob/MessyBob"),
            Option("Mid Fringe", "Avatar/Hair/midfringe/MidFringe"),
            Option("Mohawk", "Avatar/Hair/mohawk/Mohawk"),
            Option("Monk", "Avatar/Hair/monk/Monk"),
            Option("Peaked", "Avatar/Hair/peaked/Peaked"),
            Option("Receding", "Avatar/Hair/receding/Receding"),
            Option("Shoulder Length", "Avatar/Hair/shoulderlength/ShoulderLength"),
            Option("Side Part Bob", "Avatar/Hair/sidepartbob/SidePartBob"),
            Option("Spiky", "Avatar/Hair/spiky/Spiky")
        );

        public static IReadOnlyList<AppearancePresetOption> FaceLayers { get; } = Create(
            Option("Face", "Agape", "Avatar/Layers/Face/Face_Agape"),
            Option("Face", "Agited", "Avatar/Layers/Face/Face_Agited"),
            Option("Face", "Frown Pout", "Avatar/Layers/Face/Face_FrownPout"),
            Option("Face", "Neutral", "Avatar/Layers/Face/Face_Neutral"),
            Option("Face", "Neutral Pout", "Avatar/Layers/Face/Face_NeutralPout"),
            Option("Face", "Open Mouth Smile", "Avatar/Layers/Face/Face_OpenMouthSmile"),
            Option("Face", "Scared", "Avatar/Layers/Face/Face_Scared"),
            Option("Face", "Slight Frown", "Avatar/Layers/Face/Face_SlightFrown"),
            Option("Face", "Slight Smile", "Avatar/Layers/Face/Face_SlightSmile"),
            Option("Face", "Smile", "Avatar/Layers/Face/Face_Smile"),
            Option("Face", "Smug Pout", "Avatar/Layers/Face/Face_SmugPout"),
            Option("Face", "Surprised", "Avatar/Layers/Face/Face_Surprised"),
            Option("Eyes", "Eye Shadow", "Avatar/Layers/Face/EyeShadow"),
            Option("Eyes", "Freckles", "Avatar/Layers/Face/Freckles"),
            Option("Eyes", "Old Person Wrinkles", "Avatar/Layers/Face/OldPersonWrinkles"),
            Option("Eyes", "Tired Eyes", "Avatar/Layers/Face/TiredEyes"),
            Option("Facial Hair", "Goatee", "Avatar/Layers/Face/FacialHair_Goatee"),
            Option("Facial Hair", "Stubble", "Avatar/Layers/Face/FacialHair_Stubble"),
            Option("Facial Hair", "Swirl", "Avatar/Layers/Face/FacialHair_Swirl")
        );

        public static IReadOnlyList<AppearancePresetOption> BodyLayers { get; } = Create(
            Option("Top", "Button-up", "Avatar/Layers/Top/Buttonup"),
            Option("Top", "Chest Hair", "Avatar/Layers/Top/ChestHair1"),
            Option("Top", "Fast Food T-Shirt", "Avatar/Layers/Top/FastFood T-Shirt"),
            Option("Top", "Flannel Button-Up", "Avatar/Layers/Top/FlannelButtonUp"),
            Option("Top", "Gas Station T-Shirt", "Avatar/Layers/Top/GasStation T-Shirt"),
            Option("Top", "Hazmat Suit", "Avatar/Layers/Top/HazmatSuit"),
            Option("Top", "Nipples", "Avatar/Layers/Top/Nipples"),
            Option("Top", "Overalls", "Avatar/Layers/Top/Overalls"),
            Option("Top", "Rolled Button-Up", "Avatar/Layers/Top/RolledButtonUp"),
            Option("Top", "T-Shirt", "Avatar/Layers/Top/T-Shirt"),
            Option("Top", "Tucked T-Shirt", "Avatar/Layers/Top/Tucked T-Shirt"),
            Option("Top", "Upper Body Tattoos", "Avatar/Layers/Top/UpperBodyTattoos"),
            Option("Top", "V-Neck", "Avatar/Layers/Top/V-Neck"),
            Option("Bottom", "Cargo Pants", "Avatar/Layers/Bottom/CargoPants"),
            Option("Bottom", "Female Underwear", "Avatar/Layers/Bottom/FemaleUnderwear"),
            Option("Bottom", "Jeans", "Avatar/Layers/Bottom/Jeans"),
            Option("Bottom", "Jorts", "Avatar/Layers/Bottom/Jorts"),
            Option("Bottom", "Male Underwear", "Avatar/Layers/Bottom/MaleUnderwear")
        );

        public static IReadOnlyList<AppearancePresetOption> AccessoryLayers { get; } = Create(
            Option("Head", "Bucket Hat", "Avatar/Accessories/Head/BucketHat/BucketHat"),
            Option("Head", "Cap", "Avatar/Accessories/Head/Cap/Cap"),
            Option("Head", "Cap (Fast Food)", "Avatar/Accessories/Head/Cap/Cap_FastFood"),
            Option("Head", "Chef Hat", "Avatar/Accessories/Head/ChefHat/ChefHat"),
            Option("Head", "Cowboy Hat", "Avatar/Accessories/Head/Cowboy/CowboyHat"),
            Option("Head", "Flat Cap", "Avatar/Accessories/Head/FlatCap/FlatCap"),
            Option("Head", "Legend Sunglasses", "Avatar/Accessories/Head/LegendSunglasses/LegendSunglasses"),
            Option("Head", "Oakleys", "Avatar/Accessories/Head/Oakleys/Oakleys"),
            Option("Head", "Police Cap", "Avatar/Accessories/Head/PoliceCap/PoliceCap"),
            Option("Head", "Porkpie Hat", "Avatar/Accessories/Head/PorkpieHat/PorkpieHat"),
            Option("Head", "Rectangle Frame Glasses", "Avatar/Accessories/Head/RectangleFrameGlasses/RectangleFrameGlasses"),
            Option("Head", "Respirator", "Avatar/Accessories/Head/Respirator/Respirator"),
            Option("Head", "Sauce Pan", "Avatar/Accessories/Head/SaucePan/SaucePan"),
            Option("Head", "Small Round Glasses", "Avatar/Accessories/Head/SmallRoundGlasses/SmallRoundGlasses"),
            Option("Chest", "Blazer", "Avatar/Accessories/Chest/Blazer/Blazer"),
            Option("Chest", "Bullet Proof Vest", "Avatar/Accessories/Chest/BulletProofVest/BulletProofVest"),
            Option("Chest", "Bullet Proof Vest (Police)", "Avatar/Accessories/Chest/BulletProofVest/BulletProofVest_Police"),
            Option("Chest", "Collar Jacket", "Avatar/Accessories/Chest/CollarJacket/CollarJacket"),
            Option("Chest", "Open Vest", "Avatar/Accessories/Chest/OpenVest/OpenVest"),
            Option("Hands", "Polex", "Avatar/Accessories/Hands/Polex/Polex"),
            Option("Feet", "Combat Boots", "Avatar/Accessories/Feet/CombatBoots/CombatBoots"),
            Option("Feet", "Dress Shoes", "Avatar/Accessories/Feet/DressShoes/DressShoes"),
            Option("Feet", "Flats", "Avatar/Accessories/Feet/Flats/Flats"),
            Option("Feet", "Sandals", "Avatar/Accessories/Feet/Sandals/Sandals"),
            Option("Feet", "Sneakers", "Avatar/Accessories/Feet/Sneakers/Sneakers"),
            Option("Bottom", "Long Skirt", "Avatar/Accessories/Bottom/LongSkirt/LongSkirt"),
            Option("Bottom", "Medium Skirt", "Avatar/Accessories/Bottom/MediumSkirt/MediumSkirt"),
            Option("Neck", "Gold Chain", "Avatar/Accessories/Neck/GoldChain/GoldChain"),
            Option("Waist", "Apron", "Avatar/Accessories/Waist/Apron/Apron"),
            Option("Waist", "Belt", "Avatar/Accessories/Waist/Belt/Belt"),
            Option("Waist", "Hazmat Suit", "Avatar/Accessories/Waist/HazmatSuit/HazmatSuit"),
            Option("Waist", "Police Belt", "Avatar/Accessories/Waist/PoliceBelt/PoliceBelt"),
            Option("Waist", "Priest Gown", "Avatar/Accessories/Neck/PriestGown/PriestGown")
        );

        private static IReadOnlyList<AppearancePresetOption> Create(params AppearancePresetOption[] options) =>
            Array.AsReadOnly(options);

        private static AppearancePresetOption Option(string name, string path) =>
            new(name, path);

        private static AppearancePresetOption Option(string category, string name, string path) =>
            new($"{category} · {name}", path);
    }
}

