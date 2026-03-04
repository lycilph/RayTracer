namespace RayTracer.Core;

public readonly struct Ray(Vector3 origin, Vector3 direction)
{
    public readonly Vector3 Origin = origin;
    public readonly Vector3 Direction = direction;

    // The fundamental ray equation: given a distance t, where are we?
    public Vector3 At(double t) => Origin + Direction * t;
}
