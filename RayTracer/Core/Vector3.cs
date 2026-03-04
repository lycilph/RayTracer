namespace RayTracer.Core;

public readonly struct Vector3(double x, double y, double z)
{
    public static readonly Vector3 Zero = new(0, 0, 0);

    public readonly double X = x, Y = y, Z = z;

    // Arithmetic
    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator -(Vector3 v) => new(-v.X, -v.Y, -v.Z);
    public static Vector3 operator *(Vector3 v, double t) => new(v.X * t, v.Y * t, v.Z * t);
    public static Vector3 operator *(double t, Vector3 v) => v * t;
    public static Vector3 operator *(Vector3 a, Vector3 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    public static Vector3 operator /(Vector3 v, double t) => v * (1.0 / t);

    // The two most important operations in ray tracing:

    // Dot product — measures how aligned two vectors are.
    // Result is positive if they point the same way, negative if opposite, zero if perpendicular.
    // Used constantly: shading, hit detection, normal checks.
    public static double Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    // Cross product — returns a vector perpendicular to both inputs.
    // Used to build camera coordinate systems and tangent frames later.
    public static Vector3 Cross(Vector3 a, Vector3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public double LengthSquared => Dot(this, this);
    public double Length => Math.Sqrt(LengthSquared);

    // Normalized = same direction, length 1. Critical for directions —
    // many formulas only work correctly with unit vectors.
    public Vector3 Normalized => this / Length;

    public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
}
