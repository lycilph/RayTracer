using Engine.Core;

namespace Engine.Materials;

public class DielectricMaterial : IMaterial
{
    // Index of refraction relative to air (≈ 1.0).
    // Glass ≈ 1.5, diamond ≈ 2.4, water ≈ 1.33
    public double Ior { get; }

    public DielectricMaterial(double ior)
    {
        Ior = ior;
    }

    public bool Scatter(Ray rayIn, HitRecord hit, out Vector3 attenuation, out Ray scattered, Random rng)
    {
        // Glass doesn't absorb any color — attenuation is always white
        attenuation = new Vector3(1, 1, 1);

        // If we hit the front face the ray is entering glass (air → glass),
        // if the back face it's exiting (glass → air)
        double refractRatio = hit.FrontFace ? (1.0 / Ior) : Ior;

        Vector3 unitDir = rayIn.Direction.Normalized;
        double cosTheta = Math.Min(Vector3.Dot(-unitDir, hit.Normal), 1.0);
        double sinTheta = Math.Sqrt(1.0 - cosTheta * cosTheta);

        // Must reflect if Snell's law has no solution (total internal reflection),
        // or randomly reflect based on Schlick's approximation
        bool mustReflect = refractRatio * sinTheta > 1.0;
        bool schlickReflect = Schlick(cosTheta, refractRatio) > rng.NextDouble();

        Vector3 direction = (mustReflect || schlickReflect)
            ? MetalMaterial.Reflect(unitDir, hit.Normal)
            : Refract(unitDir, hit.Normal, refractRatio);

        scattered = new Ray(hit.Position, direction);
        return true;
    }

    // Snell's law in vector form — decomposes the refracted ray into
    // perpendicular and parallel components relative to the normal
    static Vector3 Refract(Vector3 uv, Vector3 normal, double refractRatio)
    {
        double cosTheta = Math.Min(Vector3.Dot(-uv, normal), 1.0);
        Vector3 perpendicular = refractRatio * (uv + cosTheta * normal);
        Vector3 parallel = -Math.Sqrt(Math.Abs(1.0 - perpendicular.LengthSquared)) * normal;
        return perpendicular + parallel;
    }

    // Schlick's approximation — probability of reflection at a given angle.
    // r0 is the reflectance at normal incidence (straight on).
    // The (1 - cosine)^5 term makes reflectance spike at grazing angles.
    static double Schlick(double cosine, double refractRatio)
    {
        double r0 = (1 - refractRatio) / (1 + refractRatio);
        r0 = r0 * r0;
        return r0 + (1 - r0) * Math.Pow(1 - cosine, 5);
    }
}