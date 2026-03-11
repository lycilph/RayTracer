namespace Engine.Core;

/// <summary>
/// Controls how the renderer samples and terminates paths.
/// Immutable — create a new instance to change settings.
/// </summary>
public record RenderSettings
{
    public int Width { get; init; } = 400;
    public int Height { get; init; } = 225;
    public int SamplesPerPixel { get; init; } = 64;
    public int MaxDepth { get; init; } = 12;
    public double Gamma { get; init; } = 2.0;

    // Convenience: aspect ratio derived from the two dimensions
    public double AspectRatio => (double)Width / Height;
}