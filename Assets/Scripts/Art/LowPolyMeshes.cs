using System.Collections.Generic;
using Freehold.Items;
using Freehold.World;
using UnityEngine;

namespace Freehold.Art
{
    /// <summary>Authored low-poly mesh recipes used by props and the player.</summary>
    public static class LowPolyMeshes
    {
        static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();

        public static Mesh GetOrBuild(string key, System.Func<Mesh> builder)
        {
            if (Cache.TryGetValue(key, out var existing) && existing != null)
            {
                return existing;
            }

            var mesh = builder();
            Cache[key] = mesh;
            return mesh;
        }

        public static Mesh Box(string name, Vector3 size, Color color)
        {
            return GetOrBuild($"box:{name}:{size}:{color}", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddBox(v, t, c, Vector3.zero, size, color);
                return MeshBuilder.Build(name, v, t, c);
            });
        }

        public static Mesh TreeTrunk(float height = 1.6f)
        {
            return GetOrBuild($"trunk:{height}", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddTaperedPrism(v, t, c, Vector3.zero, Vector3.up * height, 0.28f, 0.18f, 6, ArtPalette.Bark);
                MeshBuilder.AddBox(v, t, c, new Vector3(0.2f, height * 0.55f, 0f), new Vector3(0.5f, 0.12f, 0.12f), ArtPalette.BarkDark, Quaternion.Euler(0f, 0f, 25f));
                return MeshBuilder.Build("TreeTrunk", v, t, c);
            });
        }

        public static Mesh TreeCanopy(int variant)
        {
            return GetOrBuild($"canopy:{variant}", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                var color = variant % 3 == 0 ? ArtPalette.LeafA : variant % 3 == 1 ? ArtPalette.LeafB : ArtPalette.LeafC;
                MeshBuilder.AddPyramid(v, t, c, Vector3.zero, 1.8f, 1.8f, 1.5f, color);
                MeshBuilder.AddPyramid(v, t, c, new Vector3(0f, 0.55f, 0f), 1.25f, 1.25f, 1.1f, Color.Lerp(color, Color.white, 0.08f));
                MeshBuilder.AddPyramid(v, t, c, new Vector3(0.15f, 1.0f, -0.1f), 0.8f, 0.8f, 0.75f, Color.Lerp(color, Color.black, 0.05f));
                return MeshBuilder.Build("TreeCanopy", v, t, c);
            });
        }

        public static Mesh Rock(int variant)
        {
            return GetOrBuild($"rock:{variant}", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                var color = variant % 3 == 0 ? ArtPalette.RockA : variant % 3 == 1 ? ArtPalette.RockB : ArtPalette.RockC;
                var scale = new Vector3(0.55f + (variant % 3) * 0.08f, 0.35f + (variant % 2) * 0.08f, 0.5f + (variant % 4) * 0.05f);
                MeshBuilder.AddIcosaChunk(v, t, c, Vector3.zero, scale, color, variant * 1.7f);
                return MeshBuilder.Build("Rock", v, t, c);
            });
        }

        public static Mesh PondWater()
        {
            return GetOrBuild("pondWater", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                // Irregular octagon
                var radii = new[] { 2.9f, 3.1f, 2.7f, 3.2f, 2.8f, 3.0f, 2.6f, 3.15f };
                var start = v.Count;
                v.Add(Vector3.zero);
                c.Add(ArtPalette.PondHighlight);
                for (var i = 0; i < radii.Length; i++)
                {
                    var a = i / (float)radii.Length * Mathf.PI * 2f;
                    v.Add(new Vector3(Mathf.Cos(a) * radii[i], 0f, Mathf.Sin(a) * radii[i]));
                    c.Add(ArtPalette.Pond);
                }

                for (var i = 0; i < radii.Length; i++)
                {
                    t.Add(start);
                    t.Add(start + 1 + i);
                    t.Add(start + 1 + (i + 1) % radii.Length);
                }

                return MeshBuilder.Build("PondWater", v, t, c);
            });
        }

        public static Mesh PondBank()
        {
            return GetOrBuild("pondBank", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                var inner = new[] { 2.9f, 3.1f, 2.7f, 3.2f, 2.8f, 3.0f, 2.6f, 3.15f };
                var outer = new[] { 3.5f, 3.7f, 3.4f, 3.8f, 3.45f, 3.6f, 3.3f, 3.75f };
                var n = inner.Length;
                for (var i = 0; i < n; i++)
                {
                    var a0 = i / (float)n * Mathf.PI * 2f;
                    var a1 = (i + 1) / (float)n * Mathf.PI * 2f;
                    var i0 = new Vector3(Mathf.Cos(a0) * inner[i], 0.01f, Mathf.Sin(a0) * inner[i]);
                    var i1 = new Vector3(Mathf.Cos(a1) * inner[(i + 1) % n], 0.01f, Mathf.Sin(a1) * inner[(i + 1) % n]);
                    var o0 = new Vector3(Mathf.Cos(a0) * outer[i], 0.04f, Mathf.Sin(a0) * outer[i]);
                    var o1 = new Vector3(Mathf.Cos(a1) * outer[(i + 1) % n], 0.04f, Mathf.Sin(a1) * outer[(i + 1) % n]);
                    var s = v.Count;
                    v.Add(i0); c.Add(ArtPalette.PondEdge);
                    v.Add(i1); c.Add(ArtPalette.PondEdge);
                    v.Add(o1); c.Add(ArtPalette.Dirt);
                    v.Add(o0); c.Add(ArtPalette.Dirt);
                    t.Add(s); t.Add(s + 1); t.Add(s + 2);
                    t.Add(s); t.Add(s + 2); t.Add(s + 3);
                }

                return MeshBuilder.Build("PondBank", v, t, c);
            });
        }

        public static Mesh Stick()
        {
            return GetOrBuild("stick", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddTaperedPrism(v, t, c, Vector3.zero, Vector3.up * 0.7f, 0.035f, 0.02f, 5, ArtPalette.Stick);
                MeshBuilder.AddBox(v, t, c, new Vector3(0.05f, 0.45f, 0f), new Vector3(0.12f, 0.03f, 0.03f), ArtPalette.BarkDark, Quaternion.Euler(0f, 0f, 35f));
                return MeshBuilder.Build("Stick", v, t, c);
            });
        }

        public static Mesh StoneLoot()
        {
            return GetOrBuild("stoneLoot", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddIcosaChunk(v, t, c, Vector3.zero, new Vector3(0.18f, 0.12f, 0.16f), ArtPalette.Stone, 3.1f);
                return MeshBuilder.Build("StoneLoot", v, t, c);
            });
        }

        public static Mesh Bottle(Color liquidOrBody)
        {
            var key = $"bottle:{liquidOrBody}";
            return GetOrBuild(key, () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddTaperedPrism(v, t, c, Vector3.zero, Vector3.up * 0.32f, 0.08f, 0.07f, 8, liquidOrBody);
                MeshBuilder.AddTaperedPrism(v, t, c, Vector3.up * 0.32f, Vector3.up * 0.42f, 0.045f, 0.04f, 6, liquidOrBody);
                MeshBuilder.AddBox(v, t, c, new Vector3(0f, 0.45f, 0f), new Vector3(0.07f, 0.04f, 0.07f), ArtPalette.Cap);
                return MeshBuilder.Build("Bottle", v, t, c);
            });
        }

        public static Mesh Food(bool cooked)
        {
            return GetOrBuild(cooked ? "foodCooked" : "foodRaw", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                var color = cooked ? ArtPalette.CookedFood : ArtPalette.RawFood;
                MeshBuilder.AddIcosaChunk(v, t, c, Vector3.zero, new Vector3(0.14f, 0.08f, 0.1f), color, cooked ? 8f : 2f);
                MeshBuilder.AddBox(v, t, c, new Vector3(0.08f, 0.02f, 0f), new Vector3(0.08f, 0.05f, 0.06f), Color.Lerp(color, Color.black, 0.15f));
                return MeshBuilder.Build("Food", v, t, c);
            });
        }

        public static Mesh Axe()
        {
            return GetOrBuild("axe", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddBox(v, t, c, new Vector3(0f, 0.35f, 0f), new Vector3(0.05f, 0.7f, 0.05f), ArtPalette.AxeHandle);
                MeshBuilder.AddBox(v, t, c, new Vector3(0.12f, 0.62f, 0f), new Vector3(0.22f, 0.12f, 0.06f), ArtPalette.AxeHead);
                MeshBuilder.AddBox(v, t, c, new Vector3(0.22f, 0.62f, 0f), new Vector3(0.08f, 0.18f, 0.04f), Color.Lerp(ArtPalette.AxeHead, Color.white, 0.15f));
                return MeshBuilder.Build("Axe", v, t, c);
            });
        }

        public static Mesh Pickaxe()
        {
            return GetOrBuild("pickaxe", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddBox(v, t, c, new Vector3(0f, 0.35f, 0f), new Vector3(0.05f, 0.7f, 0.05f), ArtPalette.AxeHandle);
                MeshBuilder.AddBox(v, t, c, new Vector3(0f, 0.68f, 0f), new Vector3(0.42f, 0.07f, 0.07f), ArtPalette.PickHead);
                MeshBuilder.AddBox(v, t, c, new Vector3(-0.22f, 0.68f, 0f), new Vector3(0.1f, 0.05f, 0.05f), ArtPalette.PickHead, Quaternion.Euler(0f, 0f, 25f));
                MeshBuilder.AddBox(v, t, c, new Vector3(0.22f, 0.68f, 0f), new Vector3(0.1f, 0.05f, 0.05f), ArtPalette.PickHead, Quaternion.Euler(0f, 0f, -25f));
                return MeshBuilder.Build("Pickaxe", v, t, c);
            });
        }

        public static Mesh Knife()
        {
            return GetOrBuild("knife", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddBox(v, t, c, new Vector3(0f, 0.12f, 0f), new Vector3(0.04f, 0.22f, 0.05f), ArtPalette.KnifeHandle);
                MeshBuilder.AddBox(v, t, c, new Vector3(0f, 0.32f, 0f), new Vector3(0.03f, 0.28f, 0.02f), ArtPalette.KnifeBlade);
                MeshBuilder.AddBox(v, t, c, new Vector3(0.02f, 0.36f, 0f), new Vector3(0.015f, 0.2f, 0.01f), Color.Lerp(ArtPalette.KnifeBlade, Color.white, 0.25f));
                return MeshBuilder.Build("Knife", v, t, c);
            });
        }

        public static Mesh CampfireRing()
        {
            return GetOrBuild("campRing", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                for (var i = 0; i < 8; i++)
                {
                    var a = i / 8f * Mathf.PI * 2f;
                    var p = new Vector3(Mathf.Cos(a) * 0.55f, 0.06f, Mathf.Sin(a) * 0.55f);
                    MeshBuilder.AddIcosaChunk(v, t, c, p, new Vector3(0.16f, 0.08f, 0.12f), ArtPalette.RockB, i + 0.5f);
                }

                MeshBuilder.AddTaperedPrism(v, t, c, Vector3.zero, Vector3.up * 0.05f, 0.35f, 0.35f, 8, ArtPalette.Ash);
                return MeshBuilder.Build("CampfireRing", v, t, c);
            });
        }

        public static Mesh CampfireLog()
        {
            return GetOrBuild("campLog", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddTaperedPrism(v, t, c, new Vector3(-0.35f, 0f, 0f), new Vector3(0.35f, 0f, 0f), 0.07f, 0.06f, 6, ArtPalette.Wood);
                return MeshBuilder.Build("CampfireLog", v, t, c);
            });
        }

        public static Mesh Flame()
        {
            return GetOrBuild("flame", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddPyramid(v, t, c, Vector3.zero, 0.35f, 0.35f, 0.55f, ArtPalette.FlameOuter);
                MeshBuilder.AddPyramid(v, t, c, new Vector3(0f, 0.08f, 0f), 0.18f, 0.18f, 0.4f, ArtPalette.FlameCore);
                return MeshBuilder.Build("Flame", v, t, c);
            });
        }

        public static Mesh Limb(Vector3 size, Color color, string name)
        {
            return GetOrBuild($"limb:{name}:{size}:{color}", () =>
            {
                var v = new List<Vector3>();
                var t = new List<int>();
                var c = new List<Color>();
                MeshBuilder.AddBox(v, t, c, new Vector3(0f, -size.y * 0.5f, 0f), size, color);
                return MeshBuilder.Build(name, v, t, c);
            });
        }

        public static Mesh ForItem(ItemType type)
        {
            switch (type)
            {
                case ItemType.Axe: return Axe();
                case ItemType.Pickaxe: return Pickaxe();
                case ItemType.Knife: return Knife();
                case ItemType.PlasticBottle: return Bottle(ArtPalette.BottleEmpty);
                case ItemType.DirtyWaterBottle: return Bottle(ArtPalette.BottleDirty);
                case ItemType.CleanWaterBottle: return Bottle(ArtPalette.BottleClean);
                case ItemType.RawFood: return Food(false);
                case ItemType.CookedFood: return Food(true);
                case ItemType.Stick: return Stick();
                case ItemType.Stone: return StoneLoot();
                case ItemType.Wood: return CampfireLog();
                case ItemType.Backpack: return Box("Backpack", new Vector3(0.35f, 0.4f, 0.18f), ArtPalette.Shirt);
                default: return Box("fallback", Vector3.one * 0.2f, Color.magenta);
            }
        }

        public static Material Mat(Color color)
        {
            return PrimitiveFactory.ColorMaterial(color);
        }
    }
}
