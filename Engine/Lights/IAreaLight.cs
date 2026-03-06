using Engine.Core;

namespace Engine.Lights;

public interface IAreaLight
{
    // Sample a point on the light as seen from a surface point.
    // Returns:
    //   lightDir   — unit vector from surface point toward the sampled light point
    //   distance   — how far away the sampled point is
    //   pdf        — probability density of having sampled this direction (per solid angle)
    //   emission   — how much light this sample emits toward the surface point
    void Sample(Vector3 surfacePoint, Random rng,
        out Vector3 lightDir,
        out double distance,
        out double pdf,
        out Vector3 emission);

    // Evaluate the PDF for a ray arriving from a given direction —
    // needed by MIS to weight BRDF samples that happen to hit the light
    double Pdf(Vector3 surfacePoint, Vector3 direction);

    // Total emitted radiance in a given direction — used when a path
    // tracer ray happens to hit the light directly (BRDF sample path)
    Vector3 Emission(Vector3 hitPoint, Vector3 direction);
}