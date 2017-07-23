using System;

namespace Engine.Geometry
{
    public class Vector3
    {
        public double x;
        public double y;
        public double z;

        public Vector3(Vector3 v) : this(v.x, v.y, v.z) { }
        public Vector3(double x, double y, double z)
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

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }

        public static Vector3 operator *(Vector3 v, double s)
        {
            return new Vector3(v.x * s, v.y * s, v.z * s);
        }

        public static Vector3 operator *(double s, Vector3 v)
        {
            return new Vector3(v.x * s, v.y * s, v.z * s);
        }

        public static Vector3 operator /(Vector3 v, double s)
        {
            var inv = 1 / s;
            return new Vector3(v.x * inv, v.y * inv, v.z * inv);
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
        }

        public bool Equals(Vector3 v)
        {
            return this == v;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3)
            {
                var v = obj as Vector3;
                return this == v;
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

        public double LengthSquared()
        {
            return x * x + y * y + z * z;
        }

        public double Length()
        {
            return Math.Sqrt(LengthSquared());
        }

        public Vector3 Normalize()
        {
            return Normalize(this);
        }

        public static Vector3 Normalize(Vector3 v)
        {
            return v / v.Length();
        }

        public double Dot(Vector3 v)
        {
            return Dot(this, v);
        }

        public static double Dot(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public double AbsDot(Vector3 v)
        {
            return AbsDot(this, v);
        }

        public static double AbsDot(Vector3 v1, Vector3 v2)
        {
            return Math.Abs(Dot(v1, v2));
        }

        public Vector3 Cross(Vector3 v)
        {
            return Cross(this, v);
        }

        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }
    }
}
