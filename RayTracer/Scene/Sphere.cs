using RayTracer.Core;

namespace RayTracer.Scene;

public class Sphere(Vector3 center, double radius, Vector3 color)
{
    public Vector3 Center { get; } = center;
    public double Radius { get; } = radius;
    public Vector3 Color { get; } = color; // temporary — becomes a Material in Milestone 4

    // Returns t of the nearest hit, or -1 if no intersection.
    // tMin / tMax lets the caller restrict which part of the ray is valid.
    public double Hit(Ray ray, double tMin, double tMax)
    {
        Vector3 oc = ray.Origin - Center;

        double a = ray.Direction.LengthSquared;
        double b = Vector3.Dot(oc, ray.Direction);
        double c = oc.LengthSquared - Radius * Radius;

        double discriminant = b * b - a * c;
        if (discriminant < 0) return -1;

        double sqrtD = Math.Sqrt(discriminant);

        // Try the near root first, fall back to the far root
        double t = (-b - sqrtD) / a;
        if (t < tMin || t > tMax)
        {
            t = (-b + sqrtD) / a;
            if (t < tMin || t > tMax)
                return -1;
        }

        return t;
    }
}