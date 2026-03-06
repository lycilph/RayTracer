namespace Engine.Core;

public class Camera
{
    readonly Vector3 _origin;
    readonly Vector3 _lowerLeftCorner;
    readonly Vector3 _horizontal;
    readonly Vector3 _vertical;

    public Camera(int imageWidth,
                  int imageHeight,
                  Vector3 lookFrom,
                  Vector3 lookAt,
                  Vector3 vUp,
                  double vFovDegrees)
    {
        double aspectRatio = imageWidth / (double)imageHeight;
        double theta = vFovDegrees * Math.PI / 180.0;

        // Half-height of the viewport at unit distance from the camera
        double h = Math.Tan(theta / 2);
        double viewportHeight = 2.0 * h;
        double viewportWidth = viewportHeight * aspectRatio;

        // Build orthonormal camera basis
        Vector3 w = (lookFrom - lookAt).Normalized;   // backward
        Vector3 u = Vector3.Cross(vUp, w).Normalized; // right
        Vector3 v = Vector3.Cross(w, u);               // up

        _origin = lookFrom;
        _horizontal = viewportWidth * u;
        _vertical = viewportHeight * v;
        _lowerLeftCorner = _origin
                         - _horizontal / 2
                         - _vertical / 2
                         - w;
    }

    public Ray GetRay(double s, double t) => new(
        _origin,
        _lowerLeftCorner + s * _horizontal + t * _vertical - _origin);
}