namespace Engine.Geometry
{
    public class Point3
    {
        public double x;
        public double y;
        public double z;

        public Point3(Point3 p) : this(p.x, p.y, p.z) { }
        public Point3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double this[int i]
        {
            get
            {
                if (i == Constants.X) return x;
                if (i == Constants.Y) return y;
                return z;
            }
            set
            {
                if (i == Constants.X) { x = value; return; }
                if (i == Constants.Y) { y = value; return; }
                z = value;
            }
        }

        public static Point3 operator +(Point3 p1, Point3 p2)
        {
            return new Point3(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Point3 operator +(Point3 p, Vector3 v)
        {
            return new Point3(p.x + v.x, p.y + v.y, p.z + v.z);
        }

        public static Vector3 operator -(Point3 p1, Point3 p2)
        {
            return new Vector3(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }

        public static Point3 operator -(Point3 p, Vector3 v)
        {
            return new Point3(p.x - v.x, p.y - v.y, p.z - v.z);
        }

        public static Point3 operator *(Point3 p, double s)
        {
            return new Point3(p.x * s, p.y * s, p.z * s);
        }

        public static Point3 operator *(double s, Point3 p)
        {
            return new Point3(p.x * s, p.y * s, p.z * s);
        }

        public static Point3 operator /(Point3 p, double s)
        {
            var inv = 1 / s;
            return new Point3(p.x * inv, p.y * inv, p.z * inv);
        }

        public static bool operator ==(Point3 p1, Point3 p2)
        {
            return p1.x == p2.x && p1.y == p2.y && p1.z == p2.z;
        }

        public static bool operator !=(Point3 p1, Point3 p2)
        {
            return p1.x != p2.x || p1.y != p2.y || p1.z != p2.z;
        }

        public bool Equals(Point3 p)
        {
            return this == p;
        }

        public override bool Equals(object obj)
        {
            if (obj is Point3)
            {
                var p = obj as Point3;
                return this == p;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
    }
}
