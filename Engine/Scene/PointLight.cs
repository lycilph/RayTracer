using Engine.Core;

namespace Engine.Scene;

public class PointLight
{
    public Vector3 Position { get; }
    public Vector3 Color { get; }
    public double Intensity { get; }

    public PointLight(Vector3 position, Vector3 color, double intensity)
    {
        Position = position;
        Color = color;
        Intensity = intensity;
    }

    // Direction from a surface point toward this light — unit length
    public Vector3 DirectionFrom(Vector3 point) => (Position - point).Normalized;

    // Distance from a surface point to this light.
    // Used to avoid counting hits *beyond* the light as shadows.
    public double DistanceFrom(Vector3 point) => (Position - point).Length;
}
