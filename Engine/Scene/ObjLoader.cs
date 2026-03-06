using System.Globalization;
using Engine.Core;
using Engine.Materials;

namespace Engine.Scene;

public static class ObjLoader
{
    public static HittableList Load(string path, IMaterial material)
    {
        var vertices = new List<Vector3>();
        var list = new HittableList();

        foreach (string rawLine in File.ReadLines(path))
        {
            string line = rawLine.Trim();

            // Skip comments and empty lines
            if (line.StartsWith('#') || line.Length == 0)
                continue;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "v":
                    // Vertex position — x y z
                    if (parts.Length < 4) break;
                    vertices.Add(new Vector3(
                        double.Parse(parts[1], CultureInfo.InvariantCulture),
                        double.Parse(parts[2], CultureInfo.InvariantCulture),
                        double.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;

                case "f":
                    // Face — 3 or more vertex indices (1-based in OBJ format)
                    // Each index token may be "v", "v/vt", "v//vn", or "v/vt/vn"
                    // We only need the vertex index — everything before the first slash
                    if (parts.Length < 4) break;

                    int[] indices = new int[parts.Length - 1];
                    for (int i = 0; i < indices.Length; i++)
                    {
                        string token = parts[i + 1];
                        int slash = token.IndexOf('/');
                        string indexStr = slash >= 0 ? token[..slash] : token;
                        indices[i] = int.Parse(indexStr) - 1;  // convert to 0-based
                    }

                    // Triangulate — fan from the first vertex.
                    // A quad (4 indices) becomes 2 triangles: (0,1,2) and (0,2,3).
                    // An n-gon becomes n-2 triangles.
                    for (int i = 1; i < indices.Length - 1; i++)
                    {
                        list.Add(new Triangle(
                            vertices[indices[0]],
                            vertices[indices[i]],
                            vertices[indices[i + 1]],
                            material));
                    }
                    break;
            }
        }

        Console.WriteLine($"Loaded {path}: {vertices.Count} vertices, {list.Objects.Count} triangles");
        return list;
    }
}