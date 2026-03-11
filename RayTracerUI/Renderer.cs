using Accessibility;
using Engine.Core;
using Engine.Lights;
using Engine.Materials;
using Engine.Scene;

namespace RayTracerUI;

public class Renderer
{
    readonly RenderSettings _renderSettings;
    readonly CameraSettings _cameraSettings;
    readonly Action<byte[], int, int> _onPassComplete;
    readonly Action _onComplete;

    public Renderer(
        RenderSettings renderSettings, 
        CameraSettings cameraSettings,
        Action<byte[], int, int> onPassComplete,
        Action onComplete)
    {
        _renderSettings = renderSettings;
        _cameraSettings = cameraSettings;
        _onPassComplete = onPassComplete;
        _onComplete = onComplete;
    }

    public void Render()
    {
        // ── Materials ────────────────────────────────────────────────────────
        var white = new PBRMaterial(new Vector3(0.73, 0.73, 0.73), 0.0, 0.9);
        var red = new PBRMaterial(new Vector3(0.65, 0.05, 0.05), 0.0, 0.9);
        var green = new PBRMaterial(new Vector3(0.12, 0.45, 0.15), 0.0, 0.9);

        // ── Light ────────────────────────────────────────────────────────────
        // Classic Cornell Box light — small quad in the ceiling center
        var ceilingLight = new QuadLight(
            origin: new Vector3(213, 554, 227),
            u: new Vector3(130, 0, 0),
            v: new Vector3(0, 0, 105),
            emission: new Vector3(15, 15, 15));

        var lightSampler = new LightSampler(new IAreaLight[] { ceilingLight });

        // ── Room geometry ────────────────────────────────────────────────────
        // Cornell Box is traditionally 555 × 555 × 555 units
        var scene = new HittableList();

        // Floor — white, Y = 0
        scene.Add(new Quad(
            new Vector3(0, 0, 0),
            new Vector3(555, 0, 0),
            new Vector3(0, 0, 555),
            white));

        // Ceiling — white, Y = 555
        scene.Add(new Quad(
            new Vector3(555, 555, 555),
            new Vector3(-555, 0, 0),
            new Vector3(0, 0, -555),
            white));

        // Back wall — white, Z = 555
        scene.Add(new Quad(
            new Vector3(0, 0, 555),
            new Vector3(555, 0, 0),
            new Vector3(0, 555, 0),
            white));

        // Left wall — red, X = 555
        scene.Add(new Quad(
            new Vector3(555, 0, 0),
            new Vector3(0, 0, 555),
            new Vector3(0, 555, 0),
            red));

        // Right wall — green, X = 0
        scene.Add(new Quad(
            new Vector3(0, 0, 555),
            new Vector3(0, 0, -555),
            new Vector3(0, 555, 0),
            green));

        // ── Boxes ────────────────────────────────────────────────────────────
        // Each box is 6 quads. We build a helper to position and rotate them.
        AddBox(scene, white,
            center: new Vector3(185, 105, 169),
            sizeX: 165, sizeY: 165, sizeZ: 165,
            rotateY: -18);  // short box, rotated slightly left

        AddBox(scene, white,
            center: new Vector3(368, 230, 351),
            sizeX: 165, sizeY: 330, sizeZ: 165,
            rotateY: 15);  // tall box, rotated slightly right


        // Glass sphere — sitting on the floor in the left area
        scene.Add(new Sphere(
            new Vector3(185, 250, 169),   // centered above the short box
            60,
            new DielectricMaterial(1.5)));

        // Highly reflective sphere — sitting on the tall box
        scene.Add(new Sphere(
            new Vector3(555 - 90, 90, 120),  // centered above the tall box
            60,
            new PBRMaterial(
                new Vector3(0.95, 0.95, 0.95),  // near-white albedo
                metalness: 1.0,
                roughness: 0.02)));             // almost perfectly smooth

        // Light geometry — visible ceiling quad
        scene.Add(new EmissiveQuad(ceilingLight));

        IHittable bvh = new BVHNode(scene, new Random(42));

        // ── Camera ───────────────────────────────────────────────────────────
        // Classic Cornell Box camera — looking straight down -Z
        var camera = new Camera(_cameraSettings, _renderSettings.AspectRatio);

        // ── Render ───────────────────────────────────────────────────────
        // ── Accumulation buffer — running sum of all samples so far ──────
        var accumulator = new Vector3[_renderSettings.Height, _renderSettings.Width];
        var pixelBuffer = new byte[_renderSettings.Width * _renderSettings.Height * 3];

        var threadLocalRandom = new ThreadLocal<Random>(
            () => new Random(Guid.NewGuid().GetHashCode()));

        for (int pass = 1; pass <= _renderSettings.SamplesPerPixel; pass++)
        {
            // Each pass adds one sample per pixel across the full image
            Parallel.For(0, _renderSettings.Height, y =>
            {
                Random rng = threadLocalRandom.Value!;

                for (int x = 0; x < _renderSettings.Width; x++)
                {
                    double u = (x + rng.NextDouble()) / (_renderSettings.Width - 1);
                    double v = (_renderSettings.Height - 1 - y + rng.NextDouble()) / (_renderSettings.Height - 1);

                    Ray ray = camera.GetRay(u, v);
                    Vector3 color = RayColor(ray, _renderSettings.MaxDepth, rng, bvh, lightSampler);

                    // Thread safety — each (x, y) is written by exactly one thread
                    accumulator[y, x] += color;
                }
            });

            // Convert accumulator to display buffer — divide by pass count
            // to get the running average, then gamma correct
            for (int y = 0; y < _renderSettings.Height; y++)
                for (int x = 0; x < _renderSettings.Width; x++)
                {
                    Vector3 avg = accumulator[y, x] / pass;
                    Vector3 corrected = GammaCorrect(avg);

                    int idx = (y * _renderSettings.Width + x) * 3;
                    pixelBuffer[idx] = (byte)(255.999 * corrected.X);
                    pixelBuffer[idx + 1] = (byte)(255.999 * corrected.Y);
                    pixelBuffer[idx + 2] = (byte)(255.999 * corrected.Z);
                }

            // Only update the display every 5 passes after the first few
            if (pass <= 5 || pass % 5 == 0)
                _onPassComplete(pixelBuffer, pass, _renderSettings.SamplesPerPixel);
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

    static void AddBox(HittableList scene,
                       PBRMaterial material,
                       Vector3 center,
                       double sizeX,
                       double sizeY,
                       double sizeZ,
                       double rotateY)
    {
        // Half extents
        double hx = sizeX / 2;
        double hy = sizeY / 2;
        double hz = sizeZ / 2;

        // 8 corners in local space (unrotated, uncentered)
        Vector3[] c =
        [
            new(-hx, -hy, -hz),  // 0 left  bottom front
        new( hx, -hy, -hz),  // 1 right bottom front
        new( hx,  hy, -hz),  // 2 right top    front
        new(-hx,  hy, -hz),  // 3 left  top    front
        new(-hx, -hy,  hz),  // 4 left  bottom back
        new( hx, -hy,  hz),  // 5 right bottom back
        new( hx,  hy,  hz),  // 6 right top    back
        new(-hx,  hy,  hz),  // 7 left  top    back
    ];

        // Apply Y rotation and translation to each corner
        double rad = rotateY * Math.PI / 180.0;
        double cosR = Math.Cos(rad);
        double sinR = Math.Sin(rad);

        for (int i = 0; i < c.Length; i++)
        {
            double x = cosR * c[i].X + sinR * c[i].Z;
            double z = -sinR * c[i].X + cosR * c[i].Z;
            c[i] = center + new Vector3(x, c[i].Y, z);
        }

        // 6 faces — each as a Quad (origin, u edge, v edge)
        // Front face
        scene.Add(new Quad(c[0], c[1] - c[0], c[3] - c[0], material));
        // Back face
        scene.Add(new Quad(c[5], c[4] - c[5], c[6] - c[5], material));
        // Left face
        scene.Add(new Quad(c[4], c[0] - c[4], c[7] - c[4], material));
        // Right face
        scene.Add(new Quad(c[1], c[5] - c[1], c[2] - c[1], material));
        // Top face
        scene.Add(new Quad(c[3], c[2] - c[3], c[7] - c[3], material));
        // Bottom face
        scene.Add(new Quad(c[4], c[5] - c[4], c[0] - c[4], material));
    }
}