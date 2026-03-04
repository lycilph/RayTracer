namespace RayTracer.Core;

public readonly struct HitRecord
{
    public readonly Vector3 Position;   // world-space point where the ray hit
    public readonly Vector3 Normal;     // unit-length surface normal at that point
    public readonly double T;          // parameter along the ray

    // Whether the ray hit the outside or inside of the surface.
    // We'll need this in Milestone 4 for glass — a ray entering a sphere
    // hits the front face, a ray exiting hits the back face.
    public readonly bool FrontFace;

    public HitRecord(Ray ray, Vector3 position, Vector3 outwardNormal, double t)
    {
        Position = position;
        T = t;

        // If the ray and the outward normal point in the same direction,
        // the ray is inside the sphere — flip the normal to face the ray.
        // This ensures the normal always opposes the incoming ray direction,
        // which simplifies shading math significantly.
        FrontFace = Vector3.Dot(ray.Direction, outwardNormal) < 0;
        Normal = FrontFace ? outwardNormal : -outwardNormal;
    }
}
