namespace Engine.Core;

/// <summary>
/// Describes where the camera is and how it sees the scene.
/// Immutable — create a new instance to reposition the camera.
/// </summary>
public record CameraSettings
{
    public Vector3 LookFrom { get; init; } = new(0, 0, -1);
    public Vector3 LookAt { get; init; } = new(0, 0, 0);
    public Vector3 Up { get; init; } = new(0, 1, 0);
    public double VFovDegrees { get; init; } = 40.0;

    // Null = pinhole (no depth of field). A value enables thin-lens DoF.
    public double? Aperture { get; init; } = null;
    public double FocusDistance { get; init; } = 1.0;
}