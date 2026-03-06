using Engine.Core;

namespace Engine.Materials;

public static class MicrofacetBRDF
{
    // ── GGX Normal Distribution Function ────────────────────────────────
    // Measures how many microfacets are aligned with the half-vector h.
    // α (alpha) = roughness² — squaring gives more perceptually linear
    // roughness control (Disney's observation from their 2012 paper).
    // Returns a scalar — the higher, the more microfacets face this direction.
    public static double D_GGX(Vector3 n, Vector3 h, double roughness)
    {
        double alpha = roughness * roughness;
        double alpha2 = alpha * alpha;

        double nDotH = Math.Max(Vector3.Dot(n, h), 0);
        double nDotH2 = nDotH * nDotH;

        // GGX / Trowbridge-Reitz distribution
        double denom = nDotH2 * (alpha2 - 1.0) + 1.0;
        return alpha2 / (Math.PI * denom * denom);
    }

    // ── Smith Geometry Function ──────────────────────────────────────────
    // Models self-shadowing and masking of microfacets.
    // We compute it separately for the light direction (shadowing)
    // and view direction (masking), then multiply — that's the "Smith" part.
    public static double G_Smith(Vector3 n, Vector3 v, Vector3 l, double roughness)
    {
        double nDotV = Math.Max(Vector3.Dot(n, v), 0);
        double nDotL = Math.Max(Vector3.Dot(n, l), 0);

        return G1_Schlick(nDotV, roughness) * G1_Schlick(nDotL, roughness);
    }

    // Schlick-GGX approximation for one direction
    // k remaps roughness differently for direct vs image-based lighting —
    // we use the IBL remapping (α/2) which suits a path tracer better
    static double G1_Schlick(double nDotX, double roughness)
    {
        double alpha = roughness * roughness;
        double k = alpha / 2.0;
        return nDotX / (nDotX * (1.0 - k) + k);
    }

    // ── Fresnel — Schlick Approximation ─────────────────────────────────
    // F0 is the reflectance at normal incidence (looking straight at the surface).
    // For dielectrics (plastic, wood, skin) F0 is achromatic — typically 0.04.
    // For metals F0 is the surface color itself — metals reflect their own tint.
    // The (1 - cosTheta)^5 term drives reflectance toward 1.0 at grazing angles.
    public static Vector3 F_Schlick(double cosTheta, Vector3 f0)
    {
        double t = Math.Pow(1.0 - cosTheta, 5.0);
        return f0 + (new Vector3(1, 1, 1) - f0) * t;
    }

    // ── Full Cook-Torrance Specular BRDF ────────────────────────────────
    // Combines D, G, F into the final specular reflectance.
    // n  = surface normal
    // v  = view direction (toward camera)
    // l  = light direction (toward light / next bounce)
    // h  = half vector = normalize(v + l)
    // f0 = base reflectance at normal incidence
    public static Vector3 Specular(Vector3 n, Vector3 v, Vector3 l, Vector3 h,
                                   double roughness, Vector3 f0)
    {
        double d = D_GGX(n, h, roughness);
        double g = G_Smith(n, v, l, roughness);
        Vector3 f = F_Schlick(Math.Max(Vector3.Dot(h, v), 0), f0);

        double nDotL = Math.Max(Vector3.Dot(n, l), 0);
        double nDotV = Math.Max(Vector3.Dot(n, v), 0);

        // Denominator clamped to avoid division by zero at grazing angles
        double denom = 4.0 * nDotV * nDotL + 1e-4;

        return d * g * f / denom;
    }

    // ── Importance Sampling — GGX ────────────────────────────────────────
    // Instead of sampling a random hemisphere direction (as DiffuseMaterial does),
    // we sample directions that are likely to contribute significant energy —
    // biased toward the peak of the GGX lobe.
    // This dramatically reduces noise for rough specular surfaces.
    //
    // Returns a microfacet normal h sampled from the GGX distribution.
    // The caller reflects the view ray around h to get the scatter direction.
    public static Vector3 SampleGGX(Vector3 n, double roughness, Random rng)
    {
        double alpha = roughness * roughness;

        // Two uniform random numbers in [0, 1)
        double r1 = rng.NextDouble();
        double r2 = rng.NextDouble();

        // GGX spherical coordinates — derived by inverting the CDF
        double phi = 2.0 * Math.PI * r1;
        double cosTheta = Math.Sqrt((1.0 - r2) / (1.0 + (alpha * alpha - 1.0) * r2));
        double sinTheta = Math.Sqrt(1.0 - cosTheta * cosTheta);

        // Microfacet normal in tangent space
        var h = new Vector3(
            sinTheta * Math.Cos(phi),
            sinTheta * Math.Sin(phi),
            cosTheta);

        // Transform from tangent space to world space using the surface normal
        return TangentToWorld(h, n);
    }

    // Builds a tangent space basis around the normal and transforms a
    // tangent-space vector into world space.
    // Uses the Frisvad / Duff et al. method — numerically stable for all normals.
    static Vector3 TangentToWorld(Vector3 v, Vector3 n)
    {
        // Build two vectors perpendicular to n
        Vector3 up = Math.Abs(n.Z) < 0.999 ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0);
        Vector3 tangent = Vector3.Cross(up, n).Normalized;
        Vector3 bitangent = Vector3.Cross(n, tangent);

        // Rotate v from tangent space into world space
        return (tangent * v.X
              + bitangent * v.Y
              + n * v.Z).Normalized;
    }
}
