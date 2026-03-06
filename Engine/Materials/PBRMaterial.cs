using Engine.Core;

namespace Engine.Materials;

public class PBRMaterial : IMaterial
{
    public Vector3 Albedo { get; }
    public double Metalness { get; }
    public double Roughness { get; }

    const double MinRoughness = 0.04;

    public PBRMaterial(Vector3 albedo, double metalness, double roughness)
    {
        Albedo = albedo;
        Metalness = Math.Clamp(metalness, 0, 1);
        Roughness = Math.Clamp(roughness, MinRoughness, 1);
    }

    public bool Scatter(Ray rayIn, HitRecord hit, out Vector3 attenuation, out Ray scattered, Random rng)
    {
        Vector3 v = (-rayIn.Direction).Normalized;
        Vector3 f0 = Lerp(new Vector3(0.04, 0.04, 0.04), Albedo, Metalness);

        // Evaluate Fresnel at the surface normal — used to determine
        // split probability before we know the scatter direction
        double cosTheta = Math.Max(Vector3.Dot(hit.Normal, v), 0);
        Vector3 f = MicrofacetBRDF.F_Schlick(cosTheta, f0);
        double specularProb = Math.Clamp(Luminance(f), 0.1, 0.9);

        if (rng.NextDouble() < specularProb)
        {
            // ── Specular path ────────────────────────────────────────────
            // Sample microfacet normal from GGX, reflect incoming ray around it
            Vector3 h = MicrofacetBRDF.SampleGGX(hit.Normal, Roughness, rng);
            Vector3 l = MetalMaterial.Reflect((-v), h).Normalized;

            // Reflected ray went below the surface — absorb
            if (Vector3.Dot(l, hit.Normal) <= 0)
            {
                attenuation = Vector3.Zero;
                scattered = new Ray(hit.Position, hit.Normal);
                return false;
            }

            Vector3 hWorld = (v + l).Normalized;
            double nDotL = Math.Max(Vector3.Dot(hit.Normal, l), 0);
            double nDotV = Math.Max(Vector3.Dot(hit.Normal, v), 0);
            double vDotH = Math.Max(Vector3.Dot(v, hWorld), 0);
            double nDotH = Math.Max(Vector3.Dot(hit.Normal, hWorld), 0);

            double g = MicrofacetBRDF.G_Smith(hit.Normal, v, l, Roughness);
            Vector3 fresnel = MicrofacetBRDF.F_Schlick(vDotH, f0);

            // D cancels with GGX sampling PDF — leaving G and F only
            // Full derivation: BRDF * nDotL / pdf = F * G * vDotH / (nDotH * nDotV)
            attenuation = fresnel * g * vDotH / (nDotH * nDotV * specularProb + 1e-6);
            scattered = new Ray(hit.Position, l);
        }
        else
        {
            // ── Diffuse path ─────────────────────────────────────────────
            // Cosine-weighted hemisphere sample
            Vector3 scatterDir = hit.Normal + RandomUnitVector(rng);
            if (IsNearZero(scatterDir)) scatterDir = hit.Normal;
            Vector3 l = scatterDir.Normalized;

            // Fresnel at the actual scatter direction
            double nDotL = Math.Max(Vector3.Dot(hit.Normal, l), 0);
            Vector3 fresnel = MicrofacetBRDF.F_Schlick(nDotL, f0);
            Vector3 kD = (new Vector3(1, 1, 1) - fresnel) * (1.0 - Metalness);

            // Cosine-weighted PDF (nDotL / π) cancels with BRDF (kD * albedo / π)
            // leaving kD * albedo, compensated for the selection probability
            attenuation = kD * Albedo / (1.0 - specularProb + 1e-6);
            scattered = new Ray(hit.Position, l);
        }

        return true;
    }

    static Vector3 Lerp(Vector3 a, Vector3 b, double t) =>
        a * (1 - t) + b * t;

    static double Luminance(Vector3 c) =>
        0.2126 * c.X + 0.7152 * c.Y + 0.0722 * c.Z;

    static Vector3 RandomUnitVector(Random rng)
    {
        while (true)
        {
            var v = new Vector3(
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1);
            double lenSq = v.LengthSquared;
            if (lenSq > 1 || lenSq < 1e-160) continue;
            return v / Math.Sqrt(lenSq);
        }
    }

    static bool IsNearZero(Vector3 v) =>
        Math.Abs(v.X) < 1e-8 &&
        Math.Abs(v.Y) < 1e-8 &&
        Math.Abs(v.Z) < 1e-8;
}