using Engine.Core;
using Engine.Materials;

namespace Engine.Scene;

public class Quad : IHittable
{
    readonly Vector3 _origin;
    readonly Vector3 _u;
    readonly Vector3 _v;
    readonly Vector3 _normal;
    readonly double _d;        // plane constant: dot(normal, origin)
    readonly IMaterial _material;
    readonly bool _doubleSided;

    // Precomputed for the inside-quad test
    readonly Vector3 _w;          // cross(u, v) / |cross(u, v)|²

    public Quad(Vector3 origin, Vector3 u, Vector3 v, IMaterial material, bool doubleSided = false)
    {
        _origin = origin;
        _u = u;
        _v = v;
        _material = material;
        _doubleSided = doubleSided;

        Vector3 cross = Vector3.Cross(u, v);
        _normal = cross.Normalized;
        _d = Vector3.Dot(_normal, origin);

        // Used to project hit points into the quad's local UV space
        _w = cross / Vector3.Dot(cross, cross);
    }

    public HitRecord? Hit(Ray ray, double tMin, double tMax)
    {
        double dDotN = Vector3.Dot(ray.Direction, _normal);

        // Ray parallel to the quad plane — no hit
        if (Math.Abs(dDotN) < 1e-8) return null;

        // t to the plane
        double t = (_d - Vector3.Dot(ray.Origin, _normal)) / dDotN;
        if (t < tMin || t > tMax) return null;

        // Hit point and its projection into quad local space
        Vector3 hitPoint = ray.At(t);
        Vector3 local = hitPoint - _origin;

        double s = Vector3.Dot(_w, Vector3.Cross(local, _v));
        double r = Vector3.Dot(_w, Vector3.Cross(_u, local));

        // Outside the quad bounds
        if (s < 0 || s > 1 || r < 0 || r > 1) return null;

        // For double-sided quads, flip the normal to always face the incoming ray
        Vector3 outwardNormal = (_doubleSided && dDotN > 0) ? -_normal : _normal;
        return new HitRecord(ray, hitPoint, outwardNormal, t, _material);
    }

    public AABB BoundingBox()
    {
        // Expand to include all four corners with a small epsilon
        // to avoid zero-thickness boxes for axis-aligned quads
        const double eps = 1e-4;

        Vector3 a = _origin;
        Vector3 b = _origin + _u;
        Vector3 c = _origin + _v;
        Vector3 d = _origin + _u + _v;

        return new AABB(
            new Vector3(
                Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X)) - eps,
                Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y)) - eps,
                Math.Min(Math.Min(a.Z, b.Z), Math.Min(c.Z, d.Z)) - eps),
            new Vector3(
                Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X)) + eps,
                Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y)) + eps,
                Math.Max(Math.Max(a.Z, b.Z), Math.Max(c.Z, d.Z)) + eps));
    }
}