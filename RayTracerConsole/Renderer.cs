using Engine.Core;
using Engine.Materials;
using Engine.Output;
using Engine.Scene;

namespace RayTracerConsole;

public class Renderer
{
    // All render quality settings in one place.
    // Change these here for now — Phase 2 will make them scriptable.
    static readonly RenderSettings RenderSettings = new()
    {
        Width = 400,
        Height = 225,
        SamplesPerPixel = 100,
        MaxDepth = 10,
    };

    static readonly CameraSettings CameraSettings = new()
    {
        LookFrom = new Vector3(0, 0, 0),
        LookAt = new Vector3(0, 0, -1),
        Up = new Vector3(0, 1, 0),
        VFovDegrees = 60.0,
    };

    // Materials defined once, shared across spheres
    static readonly IMaterial RedDiffuse = new DiffuseMaterial(new Vector3(0.8, 0.2, 0.2));
    static readonly IMaterial GreenDiffuse = new DiffuseMaterial(new Vector3(0.2, 0.8, 0.2));
    static readonly IMaterial BlueDiffuse = new DiffuseMaterial(new Vector3(0.2, 0.2, 0.8));
    static readonly IMaterial GreyDiffuse = new DiffuseMaterial(new Vector3(0.8, 0.8, 0.8));

    static readonly Sphere[] Scene =
    [
        new Sphere(new Vector3( 0.0,    0.0, -1.0), 0.5,   RedDiffuse),
        new Sphere(new Vector3(-1.1,    0.0, -1.0), 0.5,   GreenDiffuse),
        new Sphere(new Vector3( 1.1,    0.0, -1.0), 0.5,   BlueDiffuse),
        new Sphere(new Vector3( 0.0, -100.5, -1.0), 100.0, GreyDiffuse),
    ];

    public void Render(string outputPath)
    {
        var pixels = new Vector3[RenderSettings.Height, RenderSettings.Width];
        var camera = new Camera(CameraSettings, RenderSettings.AspectRatio);

        // Thread-local Random — each thread gets its own instance,
        // seeded differently so they don't all produce the same sequence
        var threadLocalRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        int rowsCompleted = 0;

        Parallel.For(0, RenderSettings.Height, y =>
        {
            // Grab this thread's own Random instance
            Random rng = threadLocalRandom.Value!;

            for (int x = 0; x < RenderSettings.Width; x++)
            {
                Vector3 color = Vector3.Zero;

                for (int s = 0; s < RenderSettings.SamplesPerPixel; s++)
                {
                    double u = (x + rng.NextDouble()) / (RenderSettings.Width - 1);
                    double v = (RenderSettings.Height - 1 - y + rng.NextDouble()) / (RenderSettings.Height - 1);

                    var ray = new Ray(camera.GetRay(u, v).Origin, camera.GetRay(u, v).Direction);
                    color += RayColor(ray, RenderSettings.MaxDepth, new Vector3(1, 1, 1), rng);

                }

                pixels[y, x] = GammaCorrect(color / RenderSettings.SamplesPerPixel, RenderSettings.Gamma);
            }

            // Interlocked.Increment is an atomic add — safe across threads.
            // A plain rowsCompleted++ would be a data race.
            int completed = Interlocked.Increment(ref rowsCompleted);
            if (completed % 20 == 0 || completed == RenderSettings.Height)
                Console.Write($"\rScanlines completed: {completed}/{RenderSettings.Height}   ");
        });

        Console.WriteLine("\rDone.                              ");
        PpmWriter.Write(outputPath, pixels);
        Console.WriteLine($"Saved -> {outputPath}");
    }

    // Recursive ray color — bounces until it escapes or hits max depth
    static Vector3 RayColor(Ray ray, int depth, Vector3 throughput, Random rng)
    {
        // Exceeded bounce limit — no more light gathered along this path
        if (depth <= 0) return Vector3.Zero;

        // Terminate dim paths early — probability = 1 - max channel of throughput
        double survivalProb = Math.Clamp(Math.Max(throughput.X, Math.Max(throughput.Y, throughput.Z)), 0.1, 1.0);
        if (rng.NextDouble() > survivalProb)
            return Vector3.Zero;

        // Compensate survivors so the result stays unbiased
        throughput = throughput / survivalProb;

        HitRecord? hit = FindClosestHit(ray, 0.001, double.MaxValue);

        if (hit is not null)
        {
            // Ask the material what to do with this ray
            if (hit.Value.Material.Scatter(ray, hit.Value, out Vector3 attenuation, out Ray scattered, rng))
            {
                // Multiply by attenuation and recurse down the scattered ray
                return attenuation * RayColor(scattered, depth - 1, attenuation * throughput, rng);
            }

            // Material absorbed the ray — no light contribution
            return Vector3.Zero;
        }

        // Ray escaped to sky — this is the light source in a path tracer.
        // All light in the scene ultimately comes from here.
        Vector3 unitDir = ray.Direction.Normalized;
        double blend = 0.5 * (unitDir.Y + 1.0);
        return (1 - blend) * new Vector3(1, 1, 1)
                 + blend * new Vector3(0.5, 0.7, 1.0);
    }

    static HitRecord? FindClosestHit(Ray ray, double tMin, double tMax)
    {
        HitRecord? closest = null;
        double tBest = tMax;

        foreach (var sphere in Scene)
        {
            HitRecord? candidate = sphere.Hit(ray, tMin, tBest);
            if (candidate is not null)
            {
                closest = candidate;
                tBest = candidate.Value.T;
            }
        }

        return closest;
    }

    // Gamma is now driven by RenderSettings.Gamma rather than hardcoded as 2.0.
    // Math.Pow(x, 1/gamma) is the general form. When gamma=2, this is sqrt(x),
    // which is what the original hardcoded version did.
    static Vector3 GammaCorrect(Vector3 color, double gamma) => new(
        Math.Pow(Math.Clamp(color.X, 0, 1), 1.0 / gamma),
        Math.Pow(Math.Clamp(color.Y, 0, 1), 1.0 / gamma),
        Math.Pow(Math.Clamp(color.Z, 0, 1), 1.0 / gamma));
}