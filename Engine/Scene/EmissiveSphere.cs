using Engine.Core;
using Engine.Lights;
using Engine.Materials;

namespace Engine.Scene;

// Wraps a SphereLight as an IHittable so it appears in the scene
// and BRDF samples that hit it return the correct emission
public class EmissiveSphere : IHittable
{
    readonly Sphere _sphere;

    public EmissiveSphere(SphereLight light)
    {
        _sphere = new Sphere(
            light.Center,
            light.Radius,
            new EmissiveMaterial(light.Emission));
    }

    public HitRecord? Hit(Ray ray, double tMin, double tMax) =>
        _sphere.Hit(ray, tMin, tMax);

    public AABB BoundingBox() => _sphere.BoundingBox();
}