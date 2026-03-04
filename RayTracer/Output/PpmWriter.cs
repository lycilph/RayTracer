using RayTracer.Core;

namespace RayTracer.Output;

public static class PpmWriter
{
    public static void Write(string path, Vector3[,] pixels)
    {
        int height = pixels.GetLength(0);
        int width = pixels.GetLength(1);

        using var writer = new StreamWriter(path);

        // Header
        writer.WriteLine("P3");
        writer.WriteLine($"{width} {height}");
        writer.WriteLine("255");

        // Pixels — row by row, left to right
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var c = pixels[y, x];

                // Convert from [0, 1] floats to [0, 255] integers
                // Clamp handles any values that drift slightly out of range
                int r = (int)(255.999 * Math.Clamp(c.X, 0, 1));
                int g = (int)(255.999 * Math.Clamp(c.Y, 0, 1));
                int b = (int)(255.999 * Math.Clamp(c.Z, 0, 1));

                writer.WriteLine($"{r} {g} {b}");
            }
    }
}
