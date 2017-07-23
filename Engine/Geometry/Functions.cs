using System;

namespace Engine.Geometry
{
    public static class Functions
    {
        public static bool Quadratic(double a, double b, double c, out double t0, out double t1)
        {
            double discriminant = b * b - 4.0 * a * c;
            if (discriminant < 0.0)
            {
                t0 = double.NaN;
                t1 = double.NaN;
                return false;
            }
            double root_discriminant = Math.Sqrt(discriminant);

            double q;
            if (b < 0)
                q = -0.5 * (b - root_discriminant);
            else
                q = -0.5 * (b + root_discriminant);
            t0 = q / a;
            t1 = c / q;
            if (t0 > t1)
            {
                double temp = t0;
                t0 = t1;
                t1 = temp;
            }
            return true;
        }
    }
}
