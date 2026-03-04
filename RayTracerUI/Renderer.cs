using Engine.Core;
using Engine.Materials;
using Engine.Scene;

namespace RayTracerUI;

public class Renderer
{
    readonly int _width;
    readonly int _height;
    readonly Action<int, byte[], int, int> _onRowComplete;
    readonly Action _onComplete;

    const int SamplesPerPixel = 250;
    const int MaxDepth = 10;
    const double AspectRatio = 16.0 / 9.0;
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

    // Updated scene — showcases all three material types
    static readonly IMaterial RedDiffuse = new DiffuseMaterial(new Vector3(0.7, 0.3, 0.3));
    static readonly IMaterial GroundMat = new DiffuseMaterial(new Vector3(0.8, 0.8, 0.0));
    static readonly IMaterial Chrome = new MetalMaterial(new Vector3(0.8, 0.8, 0.8), fuzz: 0.2);
    static readonly IMaterial BrushedGold = new MetalMaterial(new Vector3(0.8, 0.6, 0.2), fuzz: 0.3);
    static readonly IMaterial Glass = new DielectricMaterial(1.5);

    static readonly Sphere[] Scene =
    [
        new Sphere(new Vector3( 0.0,    0.0, -1.0),  0.5,   RedDiffuse),   // centre — diffuse
        new Sphere(new Vector3(-1.1,    0.0, -1.0),  0.5,   Glass),        // left   — glass
        new Sphere(new Vector3(-1.1,    0.0, -1.0), -0.45,  Glass),        // left   — hollow inner
        new Sphere(new Vector3( 1.1,    0.0, -1.0),  0.5,   Chrome),//BrushedGold),  // right  — metal
        new Sphere(new Vector3( 0.0, -100.5, -1.0), 100.0,  GroundMat),    // ground
    ];

    public Renderer(int width, int height,
        Action<int, byte[], int, int> onRowComplete,
        Action onComplete)
    {
        _width = width;
        _height = height;
        _onRowComplete = onRowComplete;
        _onComplete = onComplete;
    }

    public void Render()
    {
        var threadLocalRandom = new ThreadLocal<Random>(
            () => new Random(Guid.NewGuid().GetHashCode()));

        int rowsCompleted = 0;

        Parallel.For(0, _height, y =>
        {
            Random rng = threadLocalRandom.Value!;

            // One byte[] per row — 3 bytes (R, G, B) per pixel
            var rowPixels = new byte[_width * 3];

            for (int x = 0; x < _width; x++)
            {
                Vector3 color = Vector3.Zero;

                for (int s = 0; s < SamplesPerPixel; s++)
                {
                    double u = (x + rng.NextDouble()) / (_width - 1);
                    double v = (_height - 1 - y + rng.NextDouble()) / (_height - 1);

                    var ray = new Ray(
                        CameraOrigin,
                        LowerLeftCorner + u * Horizontal + v * Vertical - CameraOrigin);

                    color += RayColor(ray, MaxDepth, new Vector3(1, 1, 1), rng);
                }

                Vector3 corrected = GammaCorrect(color / SamplesPerPixel);

                // Pack into byte[] — WriteableBitmap expects Rgb24 (3 bytes per pixel)
                int idx = x * 3;
                rowPixels[idx] = (byte)(255.999 * corrected.X);
                rowPixels[idx + 1] = (byte)(255.999 * corrected.Y);
                rowPixels[idx + 2] = (byte)(255.999 * corrected.Z);
            }

            int completed = Interlocked.Increment(ref rowsCompleted);
            _onRowComplete(y, rowPixels, completed, _height);
        });

        _onComplete();
    }

    static Vector3 RayColor(Ray ray, int depth, Vector3 throughput, Random rng)
    {
        if (depth <= 0) return Vector3.Zero;

        double survivalProb = Math.Clamp(
            Math.Max(throughput.X, Math.Max(throughput.Y, throughput.Z)),
            0.1, 1.0);

        if (rng.NextDouble() > survivalProb)
            return Vector3.Zero;

        throughput = throughput / survivalProb;

        HitRecord? hit = FindClosestHit(ray, 0.001, double.MaxValue);

        if (hit is not null)
        {
            if (hit.Value.Material.Scatter(ray, hit.Value, out Vector3 attenuation, out Ray scattered, rng))
                return attenuation * RayColor(scattered, depth - 1, throughput * attenuation, rng);

            return Vector3.Zero;
        }

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

    static Vector3 GammaCorrect(Vector3 color) => new(
        Math.Sqrt(Math.Clamp(color.X, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Y, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Z, 0, 1)));
}