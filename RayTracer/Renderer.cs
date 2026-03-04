using RayTracer.Core;
using RayTracer.Materials;
using RayTracer.Output;
using RayTracer.Scene;

namespace RayTracer;

public class Renderer
{
    const int ImageWidth = 400;
    const double AspectRatio = 16.0 / 9.0;
    const int ImageHeight = (int)(ImageWidth / AspectRatio);

    // How many rays per pixel — more = less noise, slower render.
    // Start with 10 to iterate fast, bump to 100 for a clean result.
    const int SamplesPerPixel = 100;

    // Maximum number of bounces per ray.
    // Without a cap, rays can bounce forever in enclosed spaces.
    // Russian roulette is the physically correct termination — we'll add
    // that as an optional upgrade below.
    const int MaxDepth = 10;

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
        var pixels = new Vector3[ImageHeight, ImageWidth];

        // Thread-local Random — each thread gets its own instance,
        // seeded differently so they don't all produce the same sequence
        var threadLocalRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        int rowsCompleted = 0;

        Parallel.For(0, ImageHeight, y =>
        {
            // Grab this thread's own Random instance
            Random rng = threadLocalRandom.Value!;

            for (int x = 0; x < ImageWidth; x++)
            {
                Vector3 color = Vector3.Zero;

                for (int s = 0; s < SamplesPerPixel; s++)
                {
                    double u = (x + rng.NextDouble()) / (ImageWidth - 1);
                    double v = (ImageHeight - 1 - y + rng.NextDouble()) / (ImageHeight - 1);

                    var ray = new Ray(
                        CameraOrigin,
                        LowerLeftCorner + u * Horizontal + v * Vertical - CameraOrigin);

                    color += RayColor(ray, MaxDepth, new Vector3(1, 1, 1), rng);
                }

                pixels[y, x] = GammaCorrect(color / SamplesPerPixel);
            }

            // Interlocked.Increment is an atomic add — safe across threads.
            // A plain rowsCompleted++ would be a data race.
            int completed = Interlocked.Increment(ref rowsCompleted);
            if (completed % 20 == 0 || completed == ImageHeight)
                Console.Write($"\rScanlines completed: {completed}/{ImageHeight}   ");
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

    // Gamma correction — monitors display colors with gamma ≈ 2.2.
    // Without this, the image looks too dark. We apply gamma 2 (square root)
    // as a close-enough approximation.
    static Vector3 GammaCorrect(Vector3 color) => new(
        Math.Sqrt(Math.Clamp(color.X, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Y, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Z, 0, 1)));
}