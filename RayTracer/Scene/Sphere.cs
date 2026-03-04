using RayTracer.Core;

namespace RayTracer.Scene;

public class Sphere(Vector3 center, double radius, Vector3 color)
{
    public Vector3 Center { get; } = center;
    public double Radius { get; } = radius;
    public Vector3 Color { get; } = color; // temporary — becomes a Material in Milestone 4

    // Returns a HitRecord if the ray hits within [tMin, tMax], null otherwise
    // tMin / tMax lets the caller restrict which part of the ray is valid.
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

        return new HitRecord(ray, position, outwardNormal, t);
    }
}