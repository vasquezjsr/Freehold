using Freehold.Art;
using UnityEngine;

namespace Freehold.Items
{
    public readonly struct ItemDefinition
    {
        public ItemType Type { get; }
        public string DisplayName { get; }
        public Color Color { get; }

        public ItemDefinition(ItemType type, string displayName, Color color)
        {
            Type = type;
            DisplayName = displayName;
            Color = color;
        }

        public static ItemDefinition Get(ItemType type)
        {
            switch (type)
            {
                case ItemType.Axe:
                    return new ItemDefinition(type, "Axe", ArtPalette.AxeHandle);
                case ItemType.Pickaxe:
                    return new ItemDefinition(type, "Pickaxe", ArtPalette.PickHead);
                case ItemType.Knife:
                    return new ItemDefinition(type, "Knife", ArtPalette.KnifeBlade);
                case ItemType.PlasticBottle:
                    return new ItemDefinition(type, "Empty Bottle", ArtPalette.BottleEmpty);
                case ItemType.DirtyWaterBottle:
                    return new ItemDefinition(type, "Dirty Water", ArtPalette.BottleDirty);
                case ItemType.CleanWaterBottle:
                    return new ItemDefinition(type, "Clean Water", ArtPalette.BottleClean);
                case ItemType.RawFood:
                    return new ItemDefinition(type, "Raw Food", ArtPalette.RawFood);
                case ItemType.CookedFood:
                    return new ItemDefinition(type, "Cooked Food", ArtPalette.CookedFood);
                case ItemType.Stick:
                    return new ItemDefinition(type, "Stick", ArtPalette.Stick);
                case ItemType.Stone:
                    return new ItemDefinition(type, "Stone", ArtPalette.Stone);
                default:
                    return new ItemDefinition(type, type.ToString(), Color.magenta);
            }
        }
    }
}
