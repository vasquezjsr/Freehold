using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Freehold.Art.Pack
{
    /// <summary>Minimal OBJ(+MTL Kd) loader for vendored CC0 packs. Editor/Play Mode friendly.</summary>
    public static class ObjMeshLoader
    {
        static readonly Dictionary<string, Mesh> MeshCache = new Dictionary<string, Mesh>();
        static readonly Dictionary<string, Color> MtlCache = new Dictionary<string, Color>();

        public static bool TryLoad(string absoluteObjPath, out Mesh mesh, out Color color)
        {
            mesh = null;
            color = Color.white;
            if (string.IsNullOrEmpty(absoluteObjPath) || !File.Exists(absoluteObjPath))
            {
                return false;
            }

            if (MeshCache.TryGetValue(absoluteObjPath, out mesh) && mesh != null)
            {
                color = MtlCache.TryGetValue(absoluteObjPath, out var c) ? c : Color.white;
                return true;
            }

            var lines = File.ReadAllLines(absoluteObjPath);
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var outVerts = new List<Vector3>();
            var outNormals = new List<Vector3>();
            var tris = new List<int>();
            var mtlFile = Path.ChangeExtension(absoluteObjPath, ".mtl");
            color = Color.white;
            if (File.Exists(mtlFile))
            {
                color = ParseFirstKd(mtlFile);
            }

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.StartsWith("v "))
                {
                    var p = ParseVec3(line, 2);
                    positions.Add(p);
                }
                else if (line.StartsWith("vn "))
                {
                    normals.Add(ParseVec3(line, 3));
                }
                else if (line.StartsWith("f "))
                {
                    var face = ParseFace(line);
                    if (face.Count < 3)
                    {
                        continue;
                    }

                    // Fan triangulation
                    for (var i = 1; i < face.Count - 1; i++)
                    {
                        AddCorner(face[0], positions, normals, outVerts, outNormals, tris);
                        AddCorner(face[i], positions, normals, outVerts, outNormals, tris);
                        AddCorner(face[i + 1], positions, normals, outVerts, outNormals, tris);
                    }
                }
            }

            if (outVerts.Count == 0)
            {
                return false;
            }

            mesh = new Mesh { name = Path.GetFileNameWithoutExtension(absoluteObjPath) };
            if (outVerts.Count > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(outVerts);
            mesh.SetTriangles(tris, 0);
            if (outNormals.Count == outVerts.Count)
            {
                mesh.SetNormals(outNormals);
            }
            else
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            MeshCache[absoluteObjPath] = mesh;
            MtlCache[absoluteObjPath] = color;
            return true;
        }

        static void AddCorner(
            (int v, int n) corner,
            List<Vector3> positions,
            List<Vector3> normals,
            List<Vector3> outVerts,
            List<Vector3> outNormals,
            List<int> tris)
        {
            var vi = corner.v - 1;
            if (vi < 0 || vi >= positions.Count)
            {
                return;
            }

            tris.Add(outVerts.Count);
            outVerts.Add(positions[vi]);
            if (corner.n > 0 && corner.n - 1 < normals.Count)
            {
                outNormals.Add(normals[corner.n - 1]);
            }
            else
            {
                outNormals.Add(Vector3.up);
            }
        }

        static List<(int v, int n)> ParseFace(string line)
        {
            var parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            var face = new List<(int, int)>();
            for (var i = 1; i < parts.Length; i++)
            {
                var bits = parts[i].Split('/');
                var v = int.Parse(bits[0], CultureInfo.InvariantCulture);
                var n = bits.Length >= 3 && bits[2].Length > 0 ? int.Parse(bits[2], CultureInfo.InvariantCulture) : 0;
                face.Add((v, n));
            }

            return face;
        }

        static Vector3 ParseVec3(string line, int expectedPrefixLen)
        {
            var parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            // OBJ: x z y? No — standard x y z. Unity is Y-up; Kenney OBJ is Y-up.
            var x = float.Parse(parts[1], CultureInfo.InvariantCulture);
            var y = float.Parse(parts[2], CultureInfo.InvariantCulture);
            var z = float.Parse(parts[3], CultureInfo.InvariantCulture);
            return new Vector3(x, y, z);
        }

        static Color ParseFirstKd(string mtlPath)
        {
            foreach (var raw in File.ReadAllLines(mtlPath))
            {
                var line = raw.Trim();
                if (!line.StartsWith("Kd "))
                {
                    continue;
                }

                var parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    return new Color(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture));
                }
            }

            return Color.white;
        }
    }
}
