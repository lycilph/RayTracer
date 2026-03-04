using Engine.Core;

namespace Engine.Materials;

public class MetalMaterial(Vector3 albedo, double fuzz = 0) : IMaterial
{
    public Vector3 Albedo { get; } = albedo;

    // Fuzz = 0 → perfect mirror. Fuzz = 1 → very rough metal.
    // Values above 1 make no physical sense — we clamp to [0, 1].
    public double Fuzz { get; } = Math.Clamp(fuzz, 0, 1);

    public bool Scatter(Ray rayIn, HitRecord hit, out Vector3 attenuation, out Ray scattered, Random rng)
    {
        // Reflect the incoming ray around the surface normal
        Vector3 reflected = Reflect(rayIn.Direction.Normalized, hit.Normal);

        // Perturb the reflected direction by a random vector scaled by Fuzz.
        // Fuzz = 0 leaves the reflection completely sharp.
        scattered = new Ray(hit.Position, reflected + Fuzz * RandomInUnitSphere(rng));
        attenuation = Albedo;

        // If the fuzzed ray ends up pointing into the surface, absorb it.
        // This can happen with high fuzz values at grazing angles.
        return Vector3.Dot(scattered.Direction, hit.Normal) > 0;
    }

    // The reflection formula — flips the component of v pointing into the surface.
    // v must be normalized for the geometry to work correctly.
    public static Vector3 Reflect(Vector3 v, Vector3 normal) =>
        v - 2 * Vector3.Dot(v, normal) * normal;

    // Random point inside the unit sphere — used for fuzz perturbation.
    // Unlike RandomUnitVector, we don't normalize — points closer to the
    // center produce less perturbation, which gives a natural distribution.
    static Vector3 RandomInUnitSphere(Random rng)
    {
        while (true)
        {
            var v = new Vector3(
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1);

            if (v.LengthSquared < 1) return v;
        }
    }
}