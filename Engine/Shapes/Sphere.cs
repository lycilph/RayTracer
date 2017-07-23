using Engine.Geometry;
using static Engine.Geometry.Functions;

namespace Engine.Shapes
{
    public class Sphere : Shape
    {
        public double radius;

        public Sphere(double radius)
        {
            this.radius = radius;
        }

        public override bool Intersect(Ray ray, out double t)
        {
            // Compute quadratic sphere coefficients
            double a = ray.d.x * ray.d.x + ray.d.y * ray.d.y + ray.d.z * ray.d.z;
            double b = 2 * (ray.d.x * ray.o.x + ray.d.y * ray.o.y + ray.d.z * ray.o.z);
            double c = ray.o.x * ray.o.x + ray.o.y * ray.o.y + ray.o.z * ray.o.z - radius * radius;

            // Solve quadratic equation for _t_ values
            if (!Quadratic(a, b, c, out double t0, out double t1))
            {
                t = double.PositiveInfinity;
                return false;
            }

            // Compute intersection distance along ray
            if (t0 > ray.t_max || t1 < ray.t_min)
            {
                t = double.PositiveInfinity;
                return false;
            }
            t = t0;
            if (t0 < ray.t_min)
            {
                t = t1;
                if (t1 > ray.t_max)
                {
                    t = double.PositiveInfinity;
                    return false;
                }
            }
            return true;
        }
    }
}
