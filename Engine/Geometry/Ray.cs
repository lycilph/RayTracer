namespace Engine.Geometry
{
    public class Ray
    {
        public Point3 o;
        public Vector3 d;
        public double t_min;
        public double t_max;

        public Ray()
        {
            t_min = double.Epsilon;
            t_max = double.PositiveInfinity;
        }
        public Ray(Point3 o, Vector3 d, double t_min = double.Epsilon, double t_max = double.PositiveInfinity)
        {
            this.o = o;
            this.d = d;
            this.t_min = t_min;
            this.t_max = t_max;
        }

        public Point3 this[double t]
        {
            get { return o + t * d; }
        }
    }
}
