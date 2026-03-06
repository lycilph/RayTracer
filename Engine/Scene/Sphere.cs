using Engine.Core;
using Engine.Materials;

namespace Engine.Scene;

public class Sphere(Vector3 center, double radius, IMaterial material) : IHittable
{
    public Vector3 Center { get; } = center;
    public double Radius { get; } = radius;
    public IMaterial Material { get; } = material;

    public HitRecord? Hit(Ray ray, double tMin, double tMax)
    {
        Vector3 oc = ray.Origin - Center;

        double a = ray.Direction.LengthSquared;
        double b = Vector3.Dot(oc, ray.Direction);
        double c = oc.LengthSquared - Radius * Radius;

        double discriminant = b * b - a * c;
        if (discriminant < 0) return null;

        double sqrtD = Math.Sqrt(discriminant);

        double t = (-b - sqrtD) / a;
        if (t < tMin || t > tMax)
        {
            t = (-b + sqrtD) / a;
            if (t < tMin || t > tMax)
                return null;
        }

        Vector3 position = ray.At(t);
        Vector3 outwardNormal = (position - Center) / Radius;
        return new HitRecord(ray, position, outwardNormal, t, Material);
    }

    // The tightest AABB around a sphere — simply center ± radius on all axes
    public AABB BoundingBox() => new(
        Center - new Vector3(Radius, Radius, Radius),
        Center + new Vector3(Radius, Radius, Radius));
}