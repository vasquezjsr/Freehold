using UnityEngine;

namespace Freehold.Items
{
    public readonly struct ItemDefinition
    {
        public ItemType Type { get; }
        public string DisplayName { get; }
        public Color Color { get; }
        public Vector3 Scale { get; }
        public PrimitiveType Mesh { get; }

        public ItemDefinition(ItemType type, string displayName, Color color, Vector3 scale, PrimitiveType mesh)
        {
            Type = type;
            DisplayName = displayName;
            Color = color;
            Scale = scale;
            Mesh = mesh;
        }

        public static ItemDefinition Get(ItemType type)
        {
            switch (type)
            {
                case ItemType.Axe:
                    return new ItemDefinition(type, "Axe", new Color(0.55f, 0.35f, 0.2f), new Vector3(0.25f, 0.7f, 0.15f), PrimitiveType.Cube);
                case ItemType.Pickaxe:
                    return new ItemDefinition(type, "Pickaxe", new Color(0.45f, 0.45f, 0.5f), new Vector3(0.25f, 0.7f, 0.15f), PrimitiveType.Cube);
                case ItemType.Knife:
                    return new ItemDefinition(type, "Knife", new Color(0.75f, 0.75f, 0.8f), new Vector3(0.12f, 0.45f, 0.08f), PrimitiveType.Cube);
                case ItemType.PlasticBottle:
                    return new ItemDefinition(type, "Empty Bottle", new Color(0.55f, 0.85f, 0.95f), new Vector3(0.22f, 0.4f, 0.22f), PrimitiveType.Cylinder);
                case ItemType.DirtyWaterBottle:
                    return new ItemDefinition(type, "Dirty Water", new Color(0.45f, 0.55f, 0.35f), new Vector3(0.22f, 0.4f, 0.22f), PrimitiveType.Cylinder);
                case ItemType.CleanWaterBottle:
                    return new ItemDefinition(type, "Clean Water", new Color(0.35f, 0.7f, 0.95f), new Vector3(0.22f, 0.4f, 0.22f), PrimitiveType.Cylinder);
                case ItemType.RawFood:
                    return new ItemDefinition(type, "Raw Food", new Color(0.85f, 0.35f, 0.3f), new Vector3(0.3f, 0.2f, 0.3f), PrimitiveType.Sphere);
                case ItemType.CookedFood:
                    return new ItemDefinition(type, "Cooked Food", new Color(0.7f, 0.4f, 0.2f), new Vector3(0.3f, 0.2f, 0.3f), PrimitiveType.Sphere);
                case ItemType.Stick:
                    return new ItemDefinition(type, "Stick", new Color(0.5f, 0.32f, 0.18f), new Vector3(0.08f, 0.55f, 0.08f), PrimitiveType.Cylinder);
                case ItemType.Stone:
                    return new ItemDefinition(type, "Stone", new Color(0.55f, 0.55f, 0.52f), new Vector3(0.28f, 0.22f, 0.28f), PrimitiveType.Sphere);
                default:
                    return new ItemDefinition(type, type.ToString(), Color.magenta, Vector3.one * 0.3f, PrimitiveType.Cube);
            }
        }
    }
}
