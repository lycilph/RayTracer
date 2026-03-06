using System.Diagnostics;
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

    const int SamplesPerPixel = 50;
    const int MaxDepth = 10;

    // Look-at camera parameters
    static readonly Vector3 VUp = new(0, 1, 0);    // world up
    const double VFov = 40.0;                  // vertical field of view, degrees

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
        // ── Build scene ──────────────────────────────────────────────────
        var material = new DiffuseMaterial(new Vector3(0.8, 0.7, 0.6));

        // Load OBJ — path can be made configurable later
        HittableList meshList = ObjLoader.Load(@"C:\Users\Morten Lang\source\repos\RayTracer\Models\newell_teaset\teapot.obj", material);

        AABB bounds = meshList.BoundingBox();
        Vector3 centre = (bounds.Min + bounds.Max) * 0.5;
        double size = Math.Max(
                            bounds.Max.X - bounds.Min.X,
                            Math.Max(bounds.Max.Y - bounds.Min.Y,
                                     bounds.Max.Z - bounds.Min.Z));

        Debug.WriteLine($"Mesh bounds: {bounds.Min} → {bounds.Max}");
        Debug.WriteLine($"Centre: {centre}  Size: {size:F2}");

        // Add a ground plane (large sphere trick)
        meshList.Add(new Sphere(
            new Vector3(0, -1000.5, 0), 1000,
            new DiffuseMaterial(new Vector3(0.5, 0.5, 0.5))));

        // Build BVH — this is the one-time construction cost
        var rng = new Random(42);
        IHittable scene = new BVHNode(meshList, rng);

     
        AABB meshBounds = meshList.BoundingBox();
        Debug.WriteLine($"Mesh bounds: {meshBounds.Min} → {meshBounds.Max}");
        Debug.WriteLine($"Mesh size:   X={meshBounds.Max.X - meshBounds.Min.X:F2}  Y={meshBounds.Max.Y - meshBounds.Min.Y:F2}  Z={meshBounds.Max.Z - meshBounds.Min.Z:F2}");

        // ── Build camera ─────────────────────────────────────────────────
        //var camera = new Camera(_width, _height, LookFrom, LookAt, VUp, VFov);
        var camera = new Camera(_width, _height,
            centre + new Vector3(0, size * 0.3, size * 1.5),
            centre,
            VUp,
            VFov);

        // ── Render ───────────────────────────────────────────────────────
        var threadLocalRandom = new ThreadLocal<Random>(
            () => new Random(Guid.NewGuid().GetHashCode()));

        int rowsCompleted = 0;

        Parallel.For(0, _height, y =>
        {
            Random rngLocal = threadLocalRandom.Value!;
            var rowPixels = new byte[_width * 3];

            for (int x = 0; x < _width; x++)
            {
                Vector3 color = Vector3.Zero;

                for (int s = 0; s < SamplesPerPixel; s++)
                {
                    double u = (x + rngLocal.NextDouble()) / (_width - 1);
                    double v = (_height - 1 - y + rngLocal.NextDouble()) / (_height - 1);

                    Ray ray = camera.GetRay(u, v);
                    color += RayColor(ray, MaxDepth, new Vector3(1, 1, 1), rngLocal, scene);
                }

                Vector3 corrected = GammaCorrect(color / SamplesPerPixel);

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

    static Vector3 RayColor(Ray ray, int depth, Vector3 throughput, Random rng, IHittable scene)
    {
        if (depth <= 0) return Vector3.Zero;

        double survivalProb = Math.Clamp(
            Math.Max(throughput.X, Math.Max(throughput.Y, throughput.Z)),
            0.1, 1.0);

        if (rng.NextDouble() > survivalProb)
            return Vector3.Zero;

        throughput = throughput / survivalProb;

        HitRecord? hit = scene.Hit(ray, 0.001, double.MaxValue);

        if (hit is not null)
        {
            if (hit.Value.Material.Scatter(ray, hit.Value, out Vector3 attenuation, out Ray scattered, rng))
                return attenuation * RayColor(scattered, depth - 1, throughput * attenuation, rng, scene);

            return Vector3.Zero;
        }

        // Sky
        Vector3 unitDir = ray.Direction.Normalized;
        double blend = 0.5 * (unitDir.Y + 1.0);
        return (1 - blend) * new Vector3(1, 1, 1)
                 + blend * new Vector3(0.5, 0.7, 1.0);
    }

    static Vector3 GammaCorrect(Vector3 color) => new(
        Math.Sqrt(Math.Clamp(color.X, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Y, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Z, 0, 1)));
}