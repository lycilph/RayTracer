using Engine.Core;

namespace Engine.Lights;

public class QuadLight : IAreaLight
{
    public Vector3 Origin { get; }   // one corner of the quad
    public Vector3 U { get; }   // first edge vector
    public Vector3 V { get; }   // second edge vector
    public Vector3 Emission { get; }

    readonly Vector3 _normal;     // unit normal of the quad plane
    readonly double _area;       // precomputed surface area

    public QuadLight(Vector3 origin, Vector3 u, Vector3 v, Vector3 emission)
    {
        Origin = origin;
        U = u;
        V = v;
        Emission = emission;

        // Normal from cross product of edge vectors — points toward the "front"
        // The winding order (u then v) determines which side is the front face
        Vector3 cross = Vector3.Cross(u, v);
        _area = cross.Length;         // |u × v| = area of the parallelogram
        _normal = cross / _area;        // normalize
    }

    public void Sample(Vector3 surfacePoint, Random rng,
        out Vector3 lightDir,
        out double distance,
        out double pdf,
        out Vector3 emission)
    {
        // Uniform random point on the quad surface
        double s = rng.NextDouble();
        double t = rng.NextDouble();
        Vector3 pointOnLight = Origin + s * U + t * V;

        // Direction and distance from surface to light sample
        Vector3 toLight = pointOnLight - surfacePoint;
        distance = toLight.Length;
        lightDir = toLight / distance;

        // Cosine of angle between light normal and direction toward surface
        double cosTheta = Vector3.Dot(_normal, -lightDir);

        // Light is facing away from the surface point — no contribution
        if (cosTheta <= 0)
        {
            pdf = 0;
            emission = Vector3.Zero;
            return;
        }

        // Convert area PDF to solid angle PDF:
        // pdf_area = 1 / area
        // pdf_solidAngle = pdf_area * distance² / cosTheta
        pdf = (distance * distance) / (cosTheta * _area);
        emission = Emission;
    }

    public double Pdf(Vector3 surfacePoint, Vector3 direction)
    {
        // Find where this direction intersects the quad plane
        double dDotN = Vector3.Dot(direction, _normal);

        // Ray is parallel to or facing away from the quad
        if (dDotN >= 0) return 0;

        // t along ray to the plane
        double t = Vector3.Dot(Origin - surfacePoint, _normal) / dDotN;
        if (t <= 0) return 0;

        // Hit point on the plane
        Vector3 hitPoint = surfacePoint + t * direction;

        // Check if hit point is inside the quad using barycentric-like test
        Vector3 local = hitPoint - Origin;
        double s = Vector3.Dot(local, U) / U.LengthSquared;
        double r = Vector3.Dot(local, V) / V.LengthSquared;

        if (s < 0 || s > 1 || r < 0 || r > 1) return 0;

        double cosTheta = Math.Abs(dDotN);   // already have dot(dir, normal)
        double distance = t;

        return (distance * distance) / (cosTheta * _area);
    }

    Vector3 IAreaLight.Emission(Vector3 hitPoint, Vector3 direction) => Emission;
}