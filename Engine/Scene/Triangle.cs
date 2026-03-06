using Engine.Core;
using Engine.Materials;

namespace Engine.Scene;

public class Triangle : IHittable
{
    readonly Vector3 _a, _b, _c;       // vertices
    readonly Vector3 _normal;           // face normal — precomputed
    readonly IMaterial _material;

    // Edge vectors — precomputed at construction, reused every intersection test
    readonly Vector3 _ab;   // B - A
    readonly Vector3 _ac;   // C - A

    public Triangle(Vector3 a, Vector3 b, Vector3 c, IMaterial material)
    {
        _a = a;
        _b = b;
        _c = c;
        _material = material;

        _ab = b - a;
        _ac = c - a;

        // Face normal — cross product of the two edge vectors, normalized.
        // Winding order (clockwise vs counter-clockwise) determines which
        // way the normal points — OBJ files use counter-clockwise by convention.
        _normal = Vector3.Cross(_ab, _ac).Normalized;
    }

    public HitRecord? Hit(Ray ray, double tMin, double tMax)
    {
        // Möller–Trumbore algorithm
        // h = D × AC  (cross product of ray direction and second edge)
        Vector3 h = Vector3.Cross(ray.Direction, _ac);

        // det = AB · h
        // If det is near zero the ray is parallel to the triangle — no hit.
        // If det is negative the ray hits the back face.
        double det = Vector3.Dot(_ab, h);

        // Backface culling — skip triangles facing away from the ray.
        // Remove the < 0 check if you need double-sided triangles (e.g. glass).
        if (det < 1e-8) return null;

        double invDet = 1.0 / det;

        // u — first barycentric coordinate
        // If outside [0, 1] the hit point is outside the triangle
        Vector3 s = ray.Origin - _a;
        double u = invDet * Vector3.Dot(s, h);
        if (u < 0 || u > 1) return null;

        // v — second barycentric coordinate
        // If u + v > 1 the hit point is outside the triangle
        Vector3 q = Vector3.Cross(s, _ab);
        double v = invDet * Vector3.Dot(ray.Direction, q);
        if (v < 0 || u + v > 1) return null;

        // t — distance along the ray to the hit point
        double t = invDet * Vector3.Dot(_ac, q);
        if (t < tMin || t > tMax) return null;

        Vector3 position = ray.At(t);
        return new HitRecord(ray, position, _normal, t, _material);
    }

    // Tightest AABB around the triangle — min/max of all three vertices per axis.
    // Add a small epsilon on flat triangles to avoid zero-thickness boxes,
    // which can cause the AABB slab test to produce NaN on parallel rays.
    public AABB BoundingBox()
    {
        const double eps = 1e-4;

        var min = new Vector3(
            Math.Min(_a.X, Math.Min(_b.X, _c.X)) - eps,
            Math.Min(_a.Y, Math.Min(_b.Y, _c.Y)) - eps,
            Math.Min(_a.Z, Math.Min(_b.Z, _c.Z)) - eps);

        var max = new Vector3(
            Math.Max(_a.X, Math.Max(_b.X, _c.X)) + eps,
            Math.Max(_a.Y, Math.Max(_b.Y, _c.Y)) + eps,
            Math.Max(_a.Z, Math.Max(_b.Z, _c.Z)) + eps);

        return new AABB(min, max);
    }
}