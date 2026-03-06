using Engine.Core;

namespace Engine.Materials;

public class EmissiveMaterial(Vector3 emission) : IMaterial
{
    public Vector3 Emission { get; } = emission;

    // Emissive surfaces don't scatter — they terminate the path
    public bool Scatter(Ray rayIn, HitRecord hit,
        out Vector3 attenuation, out Ray scattered, Random rng)
    {
        attenuation = Vector3.Zero;
        scattered = new Ray(hit.Position, hit.Normal);
        return false;
    }
}