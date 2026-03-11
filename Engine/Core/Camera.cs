namespace Engine.Core;

public class Camera
{
    readonly Vector3 _origin;
    readonly Vector3 _lowerLeftCorner;
    readonly Vector3 _horizontal;
    readonly Vector3 _vertical;

    public Camera(CameraSettings settings, double aspectRatio)
    {
        double theta = settings.VFovDegrees * Math.PI / 180.0;

        // Half-height of the viewport at unit distance from the camera.
        // tan(fov/2) gives us how tall the viewport is relative to its distance.
        double h = Math.Tan(theta / 2);
        double viewportHeight = 2.0 * h;
        double viewportWidth = viewportHeight * aspectRatio;

        // Build an orthonormal basis for the camera's local coordinate system.
        // w points BACKWARD (away from the scene) — cameras look down -w.
        // u points RIGHT.
        // v points UP (not necessarily the same as vUp — see below).
        Vector3 w = (settings.LookFrom - settings.LookAt).Normalized;
        Vector3 u = Vector3.Cross(settings.Up, w).Normalized;
        Vector3 v = Vector3.Cross(w, u);
        // Note: v is derived from w and u, not directly from settings.Up.
        // This is intentional — settings.Up is a hint ("roughly this way is up"),
        // not a strict constraint. The cross products ensure all three axes are
        // perfectly perpendicular, even if settings.Up wasn't quite perpendicular
        // to the look direction.

        _origin = settings.LookFrom;
        _horizontal = viewportWidth * u;
        _vertical = viewportHeight * v;
        _lowerLeftCorner = _origin
                          - _horizontal / 2
                          - _vertical / 2
                          - w;
    }

    /// <summary>
    /// Returns a ray from the camera origin through viewport position (s, t).
    /// s and t are in [0, 1] — (0,0) is bottom-left, (1,1) is top-right.
    /// </summary>
    public Ray GetRay(double s, double t) => new(
        _origin,
        _lowerLeftCorner + s * _horizontal + t * _vertical - _origin);
}