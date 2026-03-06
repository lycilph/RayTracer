using Engine.Core;
using Engine.Lights;
using Engine.Materials;
using Engine.Scene;

namespace RayTracerUI;

public class Renderer
{
    readonly int _width;
    readonly int _height;
    readonly Action<byte[], int, int> _onPassComplete;
    readonly Action _onComplete;

    const int TotalPasses = 512;
    const int MaxDepth = 12;

    public Renderer(int width, int height,
        Action<byte[], int, int> onPassComplete,
        Action onComplete)
    {
        _width = width;
        _height = height;
        _onPassComplete = onPassComplete;
        _onComplete = onComplete;
    }

    public void Render()
    {
        // ── Lights ───────────────────────────────────────────────────────
        var ceilingLight = new QuadLight(
            origin: new Vector3(-2, 4, -2),
            u: new Vector3(4, 0, 0),   // 4 units wide
            v: new Vector3(0, 0, 4),   // 4 units deep
            emission: new Vector3(10, 9, 8));   // warm white

        var fillLight = new SphereLight(
            center: new Vector3(-4, 3, -1),
            radius: 0.5,
            emission: new Vector3(4, 6, 10));    // cool blue fill

        var lightSampler = new LightSampler(new IAreaLight[] { ceilingLight, fillLight });

        // ── Scene ────────────────────────────────────────────────────────
        var scene = new HittableList();

        // PBR material grid — same as Milestone 6
        int cols = 5;
        int rows = 5;
        double spacing = 2.2;

        for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                double roughness = 0.05 + (col / (double)(cols - 1)) * 0.95;
                double metalness = row / (double)(rows - 1);
                var albedo = new Vector3(0.8, 0.5, 0.3);

                scene.Add(new Sphere(
                    new Vector3(
                        (col - cols / 2) * spacing,
                        0,
                        (row - rows / 2) * spacing),
                    0.9,
                    new PBRMaterial(albedo, metalness, roughness)));
            }

        // Ground
        scene.Add(new Sphere(
            new Vector3(0, -1001, 0), 1000,
            new PBRMaterial(new Vector3(0.5, 0.5, 0.5), 0.0, 0.8)));

        // Add light spheres as emissive geometry so they're visible
        // and so BRDF samples that hit them return the correct emission
        scene.Add(new EmissiveQuad(ceilingLight));
        scene.Add(new EmissiveSphere(fillLight));

        IHittable bvh = new BVHNode(scene, new Random(42));

        // ── Camera ───────────────────────────────────────────────────────
        var camera = new Camera(_width, _height,
            lookFrom: new Vector3(0, 6, 12),
            lookAt: new Vector3(0, 0, 0),
            vUp: new Vector3(0, 1, 0),
            vFovDegrees: 38.0);

        // ── Render ───────────────────────────────────────────────────────
        // ── Accumulation buffer — running sum of all samples so far ──────
        var accumulator = new Vector3[_height, _width];
        var pixelBuffer = new byte[_width * _height * 3];

        var threadLocalRandom = new ThreadLocal<Random>(
            () => new Random(Guid.NewGuid().GetHashCode()));

        for (int pass = 1; pass <= TotalPasses; pass++)
        {
            // Each pass adds one sample per pixel across the full image
            Parallel.For(0, _height, y =>
            {
                Random rng = threadLocalRandom.Value!;

                for (int x = 0; x < _width; x++)
                {
                    double u = (x + rng.NextDouble()) / (_width - 1);
                    double v = (_height - 1 - y + rng.NextDouble()) / (_height - 1);

                    Ray ray = camera.GetRay(u, v);
                    Vector3 color = RayColor(ray, MaxDepth, rng, bvh, lightSampler);

                    // Thread safety — each (x, y) is written by exactly one thread
                    accumulator[y, x] += color;
                }
            });

            // Convert accumulator to display buffer — divide by pass count
            // to get the running average, then gamma correct
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                {
                    Vector3 avg = accumulator[y, x] / pass;
                    Vector3 corrected = GammaCorrect(avg);

                    int idx = (y * _width + x) * 3;
                    pixelBuffer[idx] = (byte)(255.999 * corrected.X);
                    pixelBuffer[idx + 1] = (byte)(255.999 * corrected.Y);
                    pixelBuffer[idx + 2] = (byte)(255.999 * corrected.Z);
                }

            // Only update the display every 5 passes after the first few
            if (pass <= 5 || pass % 5 == 0)
                _onPassComplete(pixelBuffer, pass, TotalPasses);
        }

        _onComplete();
    }

    static Vector3 RayColor(Ray ray, int depth, Random rng,
        IHittable scene, LightSampler lightSampler)
    {
        Vector3 throughput = new(1, 1, 1);
        Vector3 radiance = Vector3.Zero;
        Ray currentRay = ray;

        // Iterative rather than recursive — avoids stack overflow at high depth
        for (int bounce = 0; bounce < depth; bounce++)
        {
            HitRecord? hit = scene.Hit(currentRay, 0.001, double.MaxValue);

            // Ray escaped — add sky contribution weighted by throughput
            if (hit is null)
            {
                radiance += throughput * SkyColor(currentRay);
                break;
            }

            // Emissive hit — a BRDF sample landed on a light source.
            // Apply MIS weight to avoid double-counting with NEE.
            if (hit.Value.Material is EmissiveMaterial emissive)
            {
                // On the first bounce (camera ray) there's no NEE to double-count
                // so we take the full emission. On subsequent bounces, MIS weights it.
                double misWeight = bounce == 0
                    ? 1.0
                    : MIS.PowerWeight(1.0, lightSampler.Lights.Length > 0
                        ? lightSampler.Lights[0].Pdf(
                            currentRay.Origin, currentRay.Direction)
                        : 0);

                radiance += throughput * emissive.Emission * misWeight;
                break;
            }

            // ── Direct lighting (NEE) ────────────────────────────────────
            // Sample lights explicitly and add their contribution.
            // This is the key addition over pure path tracing.
            Vector3 v = (-currentRay.Direction).Normalized;

            Vector3 directLight = lightSampler.Sample(
                scene, hit.Value, currentRay,
                brdfPdf: dir => ApproximateBrdfPdf(hit.Value, v, dir),
                brdfEval: dir => EvaluateBrdf(hit.Value, v, dir),
                rng);

            radiance += throughput * directLight;

            // ── Indirect bounce ──────────────────────────────────────────
            // Continue the path via BRDF sampling
            if (!hit.Value.Material.Scatter(
                    currentRay, hit.Value,
                    out Vector3 attenuation, out Ray scattered, rng))
                break;

            // Russian roulette termination
            double survivalProb = Math.Clamp(
                Math.Max(attenuation.X, Math.Max(attenuation.Y, attenuation.Z)),
                0.1, 1.0);

            if (rng.NextDouble() > survivalProb) break;

            throughput *= attenuation / survivalProb;
            currentRay = scattered;
        }

        return radiance;
    }

    // Approximate BRDF PDF for MIS weighting.
    // A full implementation would query the material directly —
    // this cosine-weighted approximation is correct for diffuse
    // and reasonable for specular surfaces.
    static double ApproximateBrdfPdf(HitRecord hit, Vector3 v, Vector3 l)
    {
        double nDotL = Math.Max(Vector3.Dot(hit.Normal, l), 0);
        return nDotL / Math.PI;
    }

    // Evaluate BRDF * nDotL for a given outgoing direction.
    // For full correctness this should call back into the material —
    // this Lambertian approximation is a reasonable starting point.
    static Vector3 EvaluateBrdf(HitRecord hit, Vector3 v, Vector3 l)
    {
        double nDotL = Math.Max(Vector3.Dot(hit.Normal, l), 0);
        if (hit.Material is PBRMaterial pbr)
        {
            // Approximate as Lambertian diffuse for the NEE eval
            Vector3 albedo = pbr.Albedo * (1.0 - pbr.Metalness);
            return albedo / Math.PI * nDotL;
        }
        return new Vector3(0.5, 0.5, 0.5) / Math.PI * nDotL;
    }

    static Vector3 SkyColor(Ray ray)
    {
        // Darker sky — the area lights are the primary illumination now
        Vector3 unitDir = ray.Direction.Normalized;
        double blend = 0.5 * (unitDir.Y + 1.0);
        return ((1 - blend) * new Vector3(0.3, 0.3, 0.3)
                  + blend * new Vector3(0.1, 0.2, 0.4)) * 0.3;
    }

    static Vector3 GammaCorrect(Vector3 color) => new(
        Math.Sqrt(Math.Clamp(color.X, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Y, 0, 1)),
        Math.Sqrt(Math.Clamp(color.Z, 0, 1)));
}