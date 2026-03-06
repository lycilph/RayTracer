using Engine.Core;

namespace Engine.Materials;

public class PBRMaterial : IMaterial
{
    // Base color of the surface — for dielectrics this is the diffuse color,
    // for metals this tints the specular reflection
    public Vector3 Albedo { get; }

    // 0 = fully dielectric (plastic), 1 = fully metallic (gold, chrome)
    public double Metalness { get; }

    // 0 = perfectly smooth mirror, 1 = completely rough / diffuse-like
    public double Roughness { get; }

    // Clamp roughness to avoid numerical issues with perfectly smooth surfaces
    const double MinRoughness = 0.04;

    public PBRMaterial(Vector3 albedo, double metalness, double roughness)
    {
        Albedo = albedo;
        Metalness = Math.Clamp(metalness, 0, 1);
        Roughness = Math.Clamp(roughness, MinRoughness, 1);
    }

    public bool Scatter(Ray rayIn, HitRecord hit, out Vector3 attenuation, out Ray scattered, Random rng)
    {
        // View direction points away from the surface toward the camera
        Vector3 v = (-rayIn.Direction).Normalized;

        // F0 — base reflectance at normal incidence.
        // Dielectrics use 0.04 (achromatic). Metals use their albedo color.
        Vector3 f0 = Lerp(new Vector3(0.04, 0.04, 0.04), Albedo, Metalness);

        // Decide whether this bounce is specular or diffuse.
        // We use the luminance of F0 as a probability — shinier surfaces
        // are more likely to take the specular path.
        // This is a stochastic form of the specular/diffuse split.
        double specularProb = Luminance(f0);
        bool doSpecular = rng.NextDouble() < specularProb;

        if (doSpecular)
        {
            // ── Specular bounce ──────────────────────────────────────────
            // Sample a microfacet normal from the GGX distribution,
            // then reflect the incoming ray around it
            Vector3 h = MicrofacetBRDF.SampleGGX(hit.Normal, Roughness, rng);
            Vector3 reflected = MetalMaterial.Reflect((-v), h);

            // If the reflected ray goes below the surface, absorb it —
            // this can happen for very rough surfaces at grazing angles
            if (Vector3.Dot(reflected, hit.Normal) <= 0)
            {
                attenuation = Vector3.Zero;
                scattered = new Ray(hit.Position, hit.Normal); // dummy, won't contribute
                return false;
            }

            Vector3 l = reflected.Normalized;
            Vector3 hWorld = (v + l).Normalized;

            // Full Cook-Torrance specular BRDF
            Vector3 specular = MicrofacetBRDF.Specular(hit.Normal, v, l, hWorld, Roughness, f0);

            // Weight by PDF and cosine term, compensate for stochastic selection
            double nDotL = Math.Max(Vector3.Dot(hit.Normal, l), 0);
            attenuation = specular * nDotL / specularProb;
            scattered = new Ray(hit.Position, l);
        }
        else
        {
            // ── Diffuse bounce ───────────────────────────────────────────
            // Cosine-weighted hemisphere sample — identical to DiffuseMaterial
            Vector3 scatterDir = hit.Normal + RandomUnitVector(rng);
            if (IsNearZero(scatterDir)) scatterDir = hit.Normal;

            Vector3 l = scatterDir.Normalized;

            // Fresnel at this angle — determines how much light goes diffuse
            double cosTheta = Math.Max(Vector3.Dot(hit.Normal, l), 0);
            Vector3 f = MicrofacetBRDF.F_Schlick(cosTheta, f0);

            // Diffuse component — energy not taken by specular, scaled by albedo
            // Metals have no diffuse term (absorbed into the surface)
            Vector3 kD = (new Vector3(1, 1, 1) - f) * (1.0 - Metalness);
            Vector3 diffuse = kD * Albedo / Math.PI;

            // Weight by PDF (cosine-weighted = nDotL / π) and compensate
            // for stochastic selection probability
            double nDotL = Math.Max(cosTheta, 0);
            double pdf = nDotL / Math.PI;
            attenuation = diffuse * nDotL / (pdf * (1.0 - specularProb));
            scattered = new Ray(hit.Position, l);
        }

        return true;
    }

    // Linear interpolation between two vectors
    static Vector3 Lerp(Vector3 a, Vector3 b, double t) =>
        a * (1 - t) + b * t;

    // Perceptual luminance — weighted sum of RGB channels
    // matches human eye sensitivity (more sensitive to green)
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