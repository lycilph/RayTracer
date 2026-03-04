using RayTracer.Core;
using RayTracer.Output;
using RayTracer.Scene;

namespace RayTracer;

public class Renderer
{
    const int ImageWidth = 400;
    const double AspectRatio = 16.0 / 9.0;
    const int ImageHeight = (int)(ImageWidth / AspectRatio);

    const double ViewportHeight = 2.0;
    const double ViewportWidth = ViewportHeight * AspectRatio;
    const double FocalLength = 1.0;

    static readonly Vector3 CameraOrigin = new(0, 0, 0);
    static readonly Vector3 Horizontal = new(ViewportWidth, 0, 0);
    static readonly Vector3 Vertical = new(0, ViewportHeight, 0);
    static readonly Vector3 LowerLeftCorner =
        CameraOrigin
        - Horizontal / 2
        - Vertical / 2
        - new Vector3(0, 0, FocalLength);

    static readonly Sphere[] Scene =
    [
        new Sphere(new Vector3( 0.0,    0.0, -1.0), 0.5,   new Vector3(1,   0.2, 0.2)),
        new Sphere(new Vector3(-1.1,    0.0, -1.0), 0.5,   new Vector3(0.2, 1,   0.2)),
        new Sphere(new Vector3( 0.4,    0.0, -1.0), 0.5,   new Vector3(0.2, 0.2, 1  )),
        new Sphere(new Vector3( 0.0, -100.5, -1.0), 100.0, new Vector3(0.8, 0.8, 0.8)),
    ];

    static readonly PointLight Light = new(
        position: new Vector3(2, 3, 0),   // above and to the right
        color: new Vector3(1, 1, 1),   // white light
        intensity: 1.0);

    // A small ambient term so surfaces in full shadow aren't pure black
    const double AmbientIntensity = 0.05;

    public void Render(string outputPath)
    {
        var pixels = new Vector3[ImageHeight, ImageWidth];

        for (int y = 0; y < ImageHeight; y++)
            for (int x = 0; x < ImageWidth; x++)
            {
                double u = x / (double)(ImageWidth - 1);
                double v = (ImageHeight - 1 - y) / (double)(ImageHeight - 1);

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
        HitRecord? closest = FindClosestHit(ray, 0.001, double.MaxValue);

        if (closest is null)
        {
            // Sky gradient — unchanged from Milestone 1
            Vector3 unitDir = ray.Direction.Normalized;
            double blend = 0.5 * (unitDir.Y + 1.0);
            return (1 - blend) * new Vector3(1, 1, 1)
                     + blend * new Vector3(0.5, 0.7, 1.0);
        }

        return Shade(closest.Value);
    }

    // Finds the closest hit across all spheres in the scene
    static HitRecord? FindClosestHit(Ray ray, double tMin, double tMax)
    {
        HitRecord? closest = null;
        double tBest = tMax;

        foreach (var sphere in Scene)
        {
            HitRecord? hit = sphere.Hit(ray, tMin, tBest);
            if (hit is not null)
            {
                closest = hit;
                tBest = hit.Value.T;  // tighten the window — discard anything further
            }
        }

        return closest;
    }

    static Vector3 Shade(HitRecord hit)
    {
        // We need to recover the sphere's color — extend HitRecord in the next milestone
        // For now we derive a color from the normal as a placeholder for non-sphere hits
        Vector3 lightDir = Light.DirectionFrom(hit.Position);

        // Lambertian diffuse — how directly is this surface facing the light?
        double diffuse = Math.Max(0, Vector3.Dot(hit.Normal, lightDir));

        // Shadow ray — shoot toward the light, stop just before reaching it
        double lightDist = Light.DistanceFrom(hit.Position);
        var shadowRay = new Ray(hit.Position, lightDir);
        bool inShadow = FindClosestHit(shadowRay, 0.001, lightDist) is not null;

        // Surface color derived from normal — remove this once materials arrive
        // Maps normal components from [-1,1] to [0,1] — a useful debug visualisation
        Vector3 surfaceColor = 0.5 * (hit.Normal + new Vector3(1, 1, 1));

        double lighting = inShadow
            ? AmbientIntensity
            : AmbientIntensity + diffuse * Light.Intensity;

        return surfaceColor * lighting * Light.Color;
    }
}
