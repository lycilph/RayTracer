using Engine.Materials;

namespace Engine.Core;

public readonly struct HitRecord
{
    public readonly Vector3 Position;
    public readonly Vector3 Normal;
    public readonly double T;
    public readonly bool FrontFace;
    public readonly IMaterial Material;   // ← new

    public HitRecord(Ray ray, Vector3 position, Vector3 outwardNormal, double t, IMaterial material)
    {
        Position = position;
        T = t;
        Material = material;             // ← new

        FrontFace = Vector3.Dot(ray.Direction, outwardNormal) < 0;
        Normal = FrontFace ? outwardNormal : -outwardNormal;
    }
}