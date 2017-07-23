using Engine.Geometry;

namespace Engine.Shapes
{
    public class Shape
    {
        public virtual bool Intersect(Ray ray, out double t)
        {
            t = double.MaxValue;
            return false;
        }
    }
}
