using Engine.Core;

namespace Engine.Lights;

public class SphereLight : IAreaLight
{
    public Vector3 Center { get; }
    public double Radius { get; }
    public Vector3 Emission { get; }   // radiance emitted per unit area

    public SphereLight(Vector3 center, double radius, Vector3 emission)
    {
        Center = center;
        Radius = radius;
        Emission = emission;
    }

    public void Sample(Vector3 surfacePoint, Random rng,
        out Vector3 lightDir,
        out double distance,
        out double pdf,
        out Vector3 emission)
    {
        // Vector from surface point to light center
        Vector3 toCenter = Center - surfacePoint;
        double distCenter = toCenter.Length;

        // Sample uniformly within the cone of directions that subtends the sphere.
        // This is more efficient than sampling the sphere surface uniformly —
        // we only generate directions that are actually visible from the surface point.
        double sinThetaMax2 = (Radius * Radius) / (distCenter * distCenter);
        double cosThetaMax = Math.Sqrt(Math.Max(0, 1 - sinThetaMax2));

        // Uniform cone sampling
        double r1 = rng.NextDouble();
        double r2 = rng.NextDouble();
        double cosTheta = 1 - r1 * (1 - cosThetaMax);
        double sinTheta = Math.Sqrt(Math.Max(0, 1 - cosTheta * cosTheta));
        double phi = 2 * Math.PI * r2;

        // Direction in local space around the toCenter axis
        var localDir = new Vector3(
            sinTheta * Math.Cos(phi),
            sinTheta * Math.Sin(phi),
            cosTheta);

        // Transform to world space using toCenter as the Z axis
        lightDir = SphericalToWorld(localDir, toCenter.Normalized);

        // Find the actual distance to the sphere surface along lightDir
        distance = DistanceToSphere(surfacePoint, lightDir);

        // PDF over solid angle — uniform over the cone
        double solidAngle = 2 * Math.PI * (1 - cosThetaMax);
        pdf = 1.0 / solidAngle;
        emission = Emission;
    }

    public double Pdf(Vector3 surfacePoint, Vector3 direction)
    {
        // If the ray doesn't intersect the sphere, pdf is zero
        double dist = DistanceToSphere(surfacePoint, direction);
        if (dist < 0) return 0;

        Vector3 toCenter = Center - surfacePoint;
        double distCenter = toCenter.Length;
        double sinThetaMax2 = (Radius * Radius) / (distCenter * distCenter);
        double cosThetaMax = Math.Sqrt(Math.Max(0, 1 - sinThetaMax2));
        double solidAngle = 2 * Math.PI * (1 - cosThetaMax);

        return 1.0 / solidAngle;
    }

    Vector3 IAreaLight.Emission(Vector3 hitPoint, Vector3 direction) => Emission;

    // Intersect a ray with the sphere — returns distance t or -1 if no hit
    double DistanceToSphere(Vector3 origin, Vector3 dir)
    {
        Vector3 oc = origin - Center;
        double a = dir.LengthSquared;
        double b = Vector3.Dot(oc, dir);
        double c = oc.LengthSquared - Radius * Radius;
        double d = b * b - a * c;
        if (d < 0) return -1;
        double t = (-b - Math.Sqrt(d)) / a;
        return t > 0 ? t : -1;
    }

    // Build an orthonormal basis around the given normal and
    // rotate a tangent-space direction into world space
    static Vector3 SphericalToWorld(Vector3 v, Vector3 normal)
    {
        Vector3 up = Math.Abs(normal.Z) < 0.999
                            ? new Vector3(0, 0, 1)
                            : new Vector3(1, 0, 0);
        Vector3 tangent = Vector3.Cross(up, normal).Normalized;
        Vector3 bitangent = Vector3.Cross(normal, tangent);

        return (tangent * v.X
              + bitangent * v.Y
              + normal * v.Z).Normalized;
    }
}