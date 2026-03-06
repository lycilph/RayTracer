using Engine.Scene;

namespace Engine.Core;

public class BVHNode : IHittable
{
    readonly IHittable _left;
    readonly IHittable _right;
    readonly AABB _box;

    public BVHNode(HittableList list, Random rng)
        : this(list.Objects, 0, list.Objects.Count, rng) { }

    // Recursive constructor — splits the object list and builds subtrees
    BVHNode(List<IHittable> objects, int start, int end, Random rng)
    {
        // Pick a random axis to split along — X=0, Y=1, Z=2
        int axis = rng.Next(0, 3);

        int span = end - start;

        if (span == 1)
        {
            // Only one object — put it in both children to avoid null checks.
            // The same object will be tested twice but never double-counted
            // since Hit returns the closest t, which is the same either way.
            _left = objects[start];
            _right = objects[start];
        }
        else if (span == 2)
        {
            // Two objects — compare and assign directly, no need to sort
            if (BoxCompare(objects[start], objects[start + 1], axis))
            {
                _left = objects[start];
                _right = objects[start + 1];
            }
            else
            {
                _left = objects[start + 1];
                _right = objects[start];
            }
        }
        else
        {
            // Sort the slice along the chosen axis, then split in the middle
            objects
                .GetRange(start, span)
                .Sort((a, b) => BoxCompare(a, b, axis) ? -1 : 1);

            // Copy sorted range back — List.GetRange returns a copy
            var sorted = objects.GetRange(start, span);
            for (int i = 0; i < span; i++)
                objects[start + i] = sorted[i];

            int mid = start + span / 2;
            _left = new BVHNode(objects, start, mid, rng);
            _right = new BVHNode(objects, mid, end, rng);
        }

        // Parent box encloses both children
        _box = AABB.Surrounding(_left.BoundingBox(), _right.BoundingBox());
    }

    public HitRecord? Hit(Ray ray, double tMin, double tMax)
    {
        // Fast rejection — if the ray misses our box, skip everything inside
        if (!_box.Hit(ray, tMin, tMax))
            return null;

        // Test both children, keeping the closest hit
        HitRecord? leftHit = _left.Hit(ray, tMin, tMax);
        HitRecord? rightHit = _right.Hit(ray, tMin, leftHit?.T ?? tMax);

        // If both hit, rightHit is already the closer one due to tightened tMax
        return rightHit ?? leftHit;
    }

    public AABB BoundingBox() => _box;

    // Returns true if a's box minimum is less than b's along the given axis
    static bool BoxCompare(IHittable a, IHittable b, int axis)
    {
        Vector3 minA = a.BoundingBox().Min;
        Vector3 minB = b.BoundingBox().Min;

        return axis switch
        {
            0 => minA.X < minB.X,
            1 => minA.Y < minB.Y,
            _ => minA.Z < minB.Z
        };
    }
}