namespace Engine.Core;

public readonly struct AABB(Vector3 min, Vector3 max)
{
    public readonly Vector3 Min = min;
    public readonly Vector3 Max = max;

    // Slab method — test ray against all three axis pairs
    public bool Hit(Ray ray, double tMin, double tMax)
    {
        // Test each axis independently, narrowing [tMin, tMax] as we go.
        // If the interval collapses to empty, the ray misses the box.
        for (int axis = 0; axis < 3; axis++)
        {
            double origin = GetComponent(ray.Origin, axis);
            double direction = GetComponent(ray.Direction, axis);
            double min = GetComponent(Min, axis);
            double max = GetComponent(Max, axis);

            // t values where the ray crosses this slab's two planes
            double invD = 1.0 / direction;
            double t0 = (min - origin) * invD;
            double t1 = (max - origin) * invD;

            // If direction is negative the ray enters from the far side —
            // swap so t0 is always the entry, t1 always the exit
            if (invD < 0) (t0, t1) = (t1, t0);

            // Narrow the valid interval
            tMin = t0 > tMin ? t0 : tMin;
            tMax = t1 < tMax ? t1 : tMax;

            // Interval is empty — ray misses this box entirely
            if (tMax <= tMin) return false;
        }

        return true;
    }

    // Returns the smallest AABB that contains both input boxes.
    // Used when building the BVH — parent nodes wrap their children.
    public static AABB Surrounding(AABB a, AABB b) => new(
        new Vector3(
            Math.Min(a.Min.X, b.Min.X),
            Math.Min(a.Min.Y, b.Min.Y),
            Math.Min(a.Min.Z, b.Min.Z)),
        new Vector3(
            Math.Max(a.Max.X, b.Max.X),
            Math.Max(a.Max.Y, b.Max.Y),
            Math.Max(a.Max.Z, b.Max.Z)));

    static double GetComponent(Vector3 v, int axis) => axis switch
    {
        0 => v.X,
        1 => v.Y,
        _ => v.Z
    };
}