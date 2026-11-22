using System.Collections.Generic;
using UnityEngine;

namespace Freehold.Art
{
    /// <summary>Small helpers for authored low-poly meshes (no Asset Store).</summary>
    public static class MeshBuilder
    {
        public static Mesh Build(string name, List<Vector3> verts, List<int> tris, List<Color> colors = null)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            if (colors != null && colors.Count == verts.Count)
            {
                mesh.SetColors(colors);
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static void AddBox(List<Vector3> verts, List<int> tris, List<Color> colors, Vector3 center, Vector3 size, Color color, Quaternion rotation)
        {
            var hx = size.x * 0.5f;
            var hy = size.y * 0.5f;
            var hz = size.z * 0.5f;
            Vector3[] local =
            {
                new Vector3(-hx, -hy, -hz),
                new Vector3(hx, -hy, -hz),
                new Vector3(hx, hy, -hz),
                new Vector3(-hx, hy, -hz),
                new Vector3(-hx, -hy, hz),
                new Vector3(hx, -hy, hz),
                new Vector3(hx, hy, hz),
                new Vector3(-hx, hy, hz)
            };

            var start = verts.Count;
            for (var i = 0; i < 8; i++)
            {
                verts.Add(center + rotation * local[i]);
                colors.Add(color);
            }

            // Clockwise winding when viewed from outside (Unity left-handed).
            int[] faces =
            {
                0, 1, 2, 0, 2, 3, // -Z
                5, 4, 7, 5, 7, 6, // +Z
                4, 0, 3, 4, 3, 7, // -X
                1, 5, 6, 1, 6, 2, // +X
                3, 2, 6, 3, 6, 7, // +Y
                4, 5, 1, 4, 1, 0  // -Y
            };

            for (var i = 0; i < faces.Length; i++)
            {
                tris.Add(start + faces[i]);
            }
        }

        public static void AddBox(List<Vector3> verts, List<int> tris, List<Color> colors, Vector3 center, Vector3 size, Color color)
        {
            AddBox(verts, tris, colors, center, size, color, Quaternion.identity);
        }

        public static void AddPyramid(List<Vector3> verts, List<int> tris, List<Color> colors, Vector3 center, float width, float depth, float height, Color color)
        {
            var start = verts.Count;
            var hw = width * 0.5f;
            var hd = depth * 0.5f;
            verts.Add(center + new Vector3(-hw, 0f, -hd));
            verts.Add(center + new Vector3(hw, 0f, -hd));
            verts.Add(center + new Vector3(hw, 0f, hd));
            verts.Add(center + new Vector3(-hw, 0f, hd));
            verts.Add(center + new Vector3(0f, height, 0f));
            for (var i = 0; i < 5; i++)
            {
                colors.Add(color);
            }

            int[] faces =
            {
                0, 1, 2, 0, 2, 3,
                0, 4, 1,
                1, 4, 2,
                2, 4, 3,
                3, 4, 0
            };
            for (var i = 0; i < faces.Length; i++)
            {
                tris.Add(start + faces[i]);
            }
        }

        public static void AddTaperedPrism(List<Vector3> verts, List<int> tris, List<Color> colors, Vector3 bottom, Vector3 top, float bottomRadius, float topRadius, int sides, Color color)
        {
            sides = Mathf.Max(3, sides);
            var start = verts.Count;
            for (var i = 0; i < sides; i++)
            {
                var a = (i / (float)sides) * Mathf.PI * 2f;
                var c = Mathf.Cos(a);
                var s = Mathf.Sin(a);
                verts.Add(bottom + new Vector3(c * bottomRadius, 0f, s * bottomRadius));
                colors.Add(color);
            }

            for (var i = 0; i < sides; i++)
            {
                var a = (i / (float)sides) * Mathf.PI * 2f;
                var c = Mathf.Cos(a);
                var s = Mathf.Sin(a);
                verts.Add(top + new Vector3(c * topRadius, 0f, s * topRadius));
                colors.Add(color);
            }

            // side faces
            for (var i = 0; i < sides; i++)
            {
                var i0 = start + i;
                var i1 = start + (i + 1) % sides;
                var i2 = start + sides + (i + 1) % sides;
                var i3 = start + sides + i;
                tris.Add(i0);
                tris.Add(i3);
                tris.Add(i1);
                tris.Add(i1);
                tris.Add(i3);
                tris.Add(i2);
            }

            // caps (fan)
            var bottomCenter = verts.Count;
            verts.Add(bottom);
            colors.Add(color);
            var topCenter = verts.Count;
            verts.Add(top);
            colors.Add(color);

            for (var i = 0; i < sides; i++)
            {
                var i0 = start + i;
                var i1 = start + (i + 1) % sides;
                tris.Add(bottomCenter);
                tris.Add(i1);
                tris.Add(i0);

                var t0 = start + sides + i;
                var t1 = start + sides + (i + 1) % sides;
                tris.Add(topCenter);
                tris.Add(t0);
                tris.Add(t1);
            }
        }

        public static void AddIcosaChunk(List<Vector3> verts, List<int> tris, List<Color> colors, Vector3 center, Vector3 scale, Color color, float seed)
        {
            // Low-poly rock: irregular octahedron / dual wedge
            var pts = new[]
            {
                new Vector3(0f, 1f, 0f),
                new Vector3(0.9f, 0.15f, 0.3f),
                new Vector3(-0.2f, 0.2f, 0.95f),
                new Vector3(-0.95f, 0.1f, -0.1f),
                new Vector3(0.15f, 0.05f, -0.9f),
                new Vector3(0.4f, -0.55f, 0.35f),
                new Vector3(-0.45f, -0.5f, 0.4f),
                new Vector3(-0.2f, -0.6f, -0.45f),
                new Vector3(0.55f, -0.45f, -0.25f)
            };

            var start = verts.Count;
            for (var i = 0; i < pts.Length; i++)
            {
                var jitter = 1f + 0.12f * Mathf.Sin(seed * 12.9898f + i * 78.233f);
                verts.Add(center + Vector3.Scale(pts[i] * jitter, scale));
                colors.Add(Color.Lerp(color, color * 0.85f, (i % 3) / 3f));
            }

            int[] faces =
            {
                0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1,
                1, 5, 2, 2, 6, 3, 3, 7, 4, 4, 8, 1,
                5, 6, 2, 6, 7, 3, 7, 8, 4, 8, 5, 1,
                5, 8, 7, 5, 7, 6
            };
            for (var i = 0; i < faces.Length; i++)
            {
                tris.Add(start + faces[i]);
            }
        }

        public static GameObject CreateMeshObject(string name, Mesh mesh, Material material, Vector3 position, bool addMeshCollider = false)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            if (addMeshCollider)
            {
                var col = go.AddComponent<MeshCollider>();
                col.sharedMesh = mesh;
            }

            return go;
        }

        public static GameObject CreatePart(string name, Transform parent, Mesh mesh, Material material, Vector3 localPos, Quaternion localRot, Vector3 localScale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
    }
}
