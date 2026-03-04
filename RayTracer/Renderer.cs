using RayTracer.Core;
using RayTracer.Output;
using RayTracer.Scene;

namespace RayTracer;

public class Renderer
{
    // Image
    const int ImageWidth = 400;
    const double AspectRatio = 16.0 / 9.0;
    const int ImageHeight = (int)(ImageWidth / AspectRatio);  // 225

    // Camera / viewport
    // ViewportHeight = 2.0 is a convention — gives a natural field of view.
    // The width follows from the aspect ratio so pixels are square.
    const double ViewportHeight = 2.0;
    const double ViewportWidth = ViewportHeight * AspectRatio;
    const double FocalLength = 1.0;  // distance from camera to viewport

    static readonly Vector3 CameraOrigin = new(0, 0, 0);
    static readonly Vector3 Horizontal = new(ViewportWidth, 0, 0);
    static readonly Vector3 Vertical = new(0, ViewportHeight, 0);

    // Bottom-left corner of the viewport in world space.
    // We step right by (u * Horizontal) and up by (v * Vertical) from here.
    static readonly Vector3 LowerLeftCorner =
        CameraOrigin
        - Horizontal / 2
        - Vertical / 2
        - new Vector3(0, 0, FocalLength);

    static readonly Sphere[] Scene =
    [
        new Sphere(new Vector3( 0.0,    0.0, -1.0), 0.5,   new Vector3(1,   0.2, 0.2)),  // red
        new Sphere(new Vector3(-1.1,    0.0, -1.0), 0.5,   new Vector3(0.2, 1,   0.2)),  // green
        new Sphere(new Vector3( 0.3,    0.0, -1.0), 0.5,   new Vector3(0.2, 0.2, 1  )),  // blue
        new Sphere(new Vector3( 0.0, -100.5, -1.0), 100.0, new Vector3(0.8, 0.8, 0.8)),  // ground
    ];

    public void Render(string outputPath)
    {
        var pixels = new Vector3[ImageHeight, ImageWidth];

        for (int y = 0; y < ImageHeight; y++)
            for (int x = 0; x < ImageWidth; x++)
            {
                // u, v are [0, 1] coordinates across the viewport
                double u = x / (double)(ImageWidth - 1);
                double v = (ImageHeight - 1 - y) / (double)(ImageHeight - 1);  // flip Y — PPM row 0 is top, but viewport v=0 is bottom

                var ray = new Ray(
                    CameraOrigin,
                    LowerLeftCorner + u * Horizontal + v * Vertical - CameraOrigin);

                pixels[y, x] = RayColor(ray);
            }

        PpmWriter.Write(outputPath, pixels);
        Console.WriteLine($"Rendered {ImageWidth}x{ImageHeight} -> {outputPath}");
    }

    static Vector3 RayColor(Ray ray)
    {
        // Find the closest sphere hit along this ray
        double closest = double.MaxValue;
        Sphere? hitSphere = null;

        foreach (var sphere in Scene)
        {
            double t = sphere.Hit(ray, 0.001, closest);
            if (t > 0)
            {
                closest = t;
                hitSphere = sphere;
            }
        }

        // Hit something — return its flat color (no shading yet)
        if (hitSphere is not null)
            return hitSphere.Color;

        // Miss — sky gradient from white (bottom) to light blue (top)
        Vector3 unitDir = ray.Direction.Normalized;
        double blend = 0.5 * (unitDir.Y + 1.0);  // remap Y from [-1,1] to [0,1]
        return (1 - blend) * new Vector3(1, 1, 1)
                 + blend * new Vector3(0.5, 0.7, 1.0);
    }
}
