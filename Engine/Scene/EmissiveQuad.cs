using Engine.Core;
using Engine.Lights;
using Engine.Materials;

namespace Engine.Scene;

public class EmissiveQuad : IHittable
{
    readonly Quad _quad;

    public EmissiveQuad(QuadLight light)
    {
        _quad = new Quad(
            light.Origin,
            light.U,
            light.V,
            new EmissiveMaterial(light.Emission));
    }

    public HitRecord? Hit(Ray ray, double tMin, double tMax) =>
        _quad.Hit(ray, tMin, tMax);

    public AABB BoundingBox() => _quad.BoundingBox();
}
