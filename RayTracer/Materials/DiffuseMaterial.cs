using RayTracer.Core;

namespace RayTracer.Materials;

public class DiffuseMaterial(Vector3 albedo) : IMaterial
{
    // Attenuation = surface color. How much of each channel survives this bounce.
    public Vector3 Albedo { get; } = albedo;

    public bool Scatter(Ray rayIn, HitRecord hit, out Vector3 attenuation, out Ray scattered)
    {
        // Cosine-weighted random direction in the hemisphere around the normal
        Vector3 scatterDir = hit.Normal + RandomUnitVector();

        // Guard against degenerate scatter direction — if the random vector
        // nearly cancels the normal, the direction approaches zero length.
        // That produces NaNs downstream, so fall back to the normal itself.
        if (IsNearZero(scatterDir))
            scatterDir = hit.Normal;

        scattered = new Ray(hit.Position, scatterDir);
        attenuation = Albedo;
        return true;   // diffuse surfaces always scatter, never absorb
    }

    // Picks a random point on the surface of a unit sphere.
    // Uses rejection sampling — generate random points in the unit cube,
    // reject any that fall outside the unit sphere. On average ~1.9 attempts.
    static Vector3 RandomUnitVector()
    {
        while (true)
        {
            var v = new Vector3(
                Random.Shared.NextDouble() * 2 - 1,
                Random.Shared.NextDouble() * 2 - 1,
                Random.Shared.NextDouble() * 2 - 1);

            double lenSq = v.LengthSquared;

            // Reject points outside the sphere and points too close to origin
            if (lenSq > 1 || lenSq < 1e-160) continue;

            return v / Math.Sqrt(lenSq);  // normalize to surface of sphere
        }
    }

    // A vector is near zero if all components are very close to zero
    static bool IsNearZero(Vector3 v) =>
        Math.Abs(v.X) < 1e-8 &&
        Math.Abs(v.Y) < 1e-8 &&
        Math.Abs(v.Z) < 1e-8;
}