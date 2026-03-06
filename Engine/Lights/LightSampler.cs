using Engine.Core;
using Engine.Scene;

namespace Engine.Lights;

public class LightSampler
{
    readonly IAreaLight[] _lights;

    public LightSampler(IAreaLight[] lights)
    {
        _lights = lights;
    }

    // Evaluate direct lighting at a surface hit using NEE + MIS.
    // scene     — used to cast shadow rays
    // hit       — the surface point being shaded
    // rayIn     — the incoming ray that produced this hit
    // brdfPdf   — function returning the BRDF's pdf for a given outgoing direction
    // brdfEval  — function returning the BRDF value * nDotL for a given direction
    public Vector3 Sample(
        IHittable scene,
        HitRecord hit,
        Ray rayIn,
        Func<Vector3, double> brdfPdf,
        Func<Vector3, Vector3> brdfEval,
        Random rng)
    {
        if (_lights.Length == 0) return Vector3.Zero;

        Vector3 total = Vector3.Zero;

        foreach (var light in _lights)
        {
            // ── Light sampling strategy ──────────────────────────────────
            light.Sample(hit.Position, rng,
                out Vector3 lightDir,
                out double lightDist,
                out double lightPdf,
                out Vector3 emission);

            if (lightPdf <= 0) continue;

            // Cast shadow ray — only contribute if the light is visible
            var shadowRay = new Ray(hit.Position, lightDir);
            bool occluded = scene.Hit(shadowRay, 0.001, lightDist - 0.001) is not null;
            if (occluded) continue;

            // Evaluate the BRDF for this light direction
            Vector3 brdf = brdfEval(lightDir);
            double bPdf = brdfPdf(lightDir);

            // MIS weight — how much credit does light sampling get
            // for this direction vs what BRDF sampling would give?
            double weight = MIS.PowerWeight(lightPdf, bPdf);

            // Direct light contribution:
            // emission * brdf * misWeight / lightPdf
            total += emission * brdf * weight / lightPdf;
        }

        return total;
    }

    // Evaluate the MIS weight for a BRDF sample that happened to hit a light.
    // Called from RayColor when a bounced ray hits a light source directly.
    // Without this, BRDF samples that hit lights would be double-counted.
    public double BrdfHitWeight(Vector3 surfacePoint, Vector3 direction, IAreaLight light)
    {
        double lightPdf = light.Pdf(surfacePoint, direction);
        double brdfPdf = 1.0;  // caller supplies actual BRDF pdf — see usage in Renderer

        return MIS.PowerWeight(brdfPdf, lightPdf);
    }

    public IAreaLight[] Lights => _lights;
}