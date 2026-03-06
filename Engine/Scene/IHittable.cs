using Engine.Core;

namespace Engine.Scene;

public interface IHittable
{
    // Test a ray against this object within [tMin, tMax].
    // Returns null if no hit is found.
    HitRecord? Hit(Ray ray, double tMin, double tMax);

    // Returns the axis-aligned bounding box that fully encloses this object.
    // Every hittable must be bounded — unbounded primitives like infinite
    // planes need special handling and won't fit naturally into a BVH.
    AABB BoundingBox();
}
