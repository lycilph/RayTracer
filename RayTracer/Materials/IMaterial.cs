using RayTracer.Core;

namespace RayTracer.Materials;

public interface IMaterial
{
    // Returns false if the ray is absorbed (no scatter).
    // attenuation — how much each color channel is multiplied by this bounce.
    // scattered   — the new ray to continue tracing.
    bool Scatter(Ray rayIn, HitRecord hit, out Vector3 attenuation, out Ray scattered);
}