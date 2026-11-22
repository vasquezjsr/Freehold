using Freehold.Art;
using UnityEngine;

namespace Freehold.Items
{
    public readonly struct ItemDefinition
    {
        public ItemType Type { get; }
        public string DisplayName { get; }
        public Color Color { get; }
        /// <summary>Fits in pockets (and backpack overflow).</summary>
        public bool IsPocketSized { get; }
        /// <summary>Tools / bulk — backpack only.</summary>
        public bool RequiresBackpack { get; }

        public ItemDefinition(ItemType type, string displayName, Color color, bool isPocketSized, bool requiresBackpack)
        {
            Type = type;
            DisplayName = displayName;
            Color = color;
            IsPocketSized = isPocketSized;
            RequiresBackpack = requiresBackpack;
        }

        public static ItemDefinition Get(ItemType type)
        {
            switch (type)
            {
                case ItemType.Axe:
                    return new ItemDefinition(type, "Axe", ArtPalette.AxeHandle, false, true);
                case ItemType.Pickaxe:
                    return new ItemDefinition(type, "Pickaxe", ArtPalette.PickHead, false, true);
                case ItemType.Knife:
                    return new ItemDefinition(type, "Knife", ArtPalette.KnifeBlade, true, false);
                case ItemType.PlasticBottle:
                    return new ItemDefinition(type, "Empty Bottle", ArtPalette.BottleEmpty, true, false);
                case ItemType.DirtyWaterBottle:
                    return new ItemDefinition(type, "Dirty Water", ArtPalette.BottleDirty, true, false);
                case ItemType.CleanWaterBottle:
                    return new ItemDefinition(type, "Clean Water", ArtPalette.BottleClean, true, false);
                case ItemType.RawFood:
                    return new ItemDefinition(type, "Raw Food", ArtPalette.RawFood, true, false);
                case ItemType.CookedFood:
                    return new ItemDefinition(type, "Cooked Food", ArtPalette.CookedFood, true, false);
                case ItemType.Stick:
                    return new ItemDefinition(type, "Stick", ArtPalette.Stick, true, false);
                case ItemType.Stone:
                    return new ItemDefinition(type, "Stone", ArtPalette.Stone, true, false);
                case ItemType.Wood:
                    return new ItemDefinition(type, "Wood", ArtPalette.Wood, false, true);
                case ItemType.Backpack:
                    return new ItemDefinition(type, "Backpack", ArtPalette.Shirt, false, false);
                default:
                    return new ItemDefinition(type, type.ToString(), Color.magenta, true, false);
            }
        }
    }
}
