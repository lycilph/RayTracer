using Engine.Core;

namespace Engine.Scene;

public class HittableList : IHittable
{
    readonly List<IHittable> _objects = [];

    public void Add(IHittable obj) => _objects.Add(obj);

    public HitRecord? Hit(Ray ray, double tMin, double tMax)
    {
        HitRecord? closest = null;
        double tBest = tMax;

        foreach (var obj in _objects)
        {
            HitRecord? hit = obj.Hit(ray, tMin, tBest);
            if (hit is not null)
            {
                closest = hit;
                tBest = hit.Value.T;
            }
        }

        return closest;
    }

    // The bounding box of a list is the box that contains all members.
    // We grow it incrementally — start with the first object's box,
    // then expand to include each subsequent one.
    public AABB BoundingBox()
    {
        if (_objects.Count == 0)
            throw new InvalidOperationException("Cannot get bounding box of empty list.");

        AABB result = _objects[0].BoundingBox();

        for (int i = 1; i < _objects.Count; i++)
            result = AABB.Surrounding(result, _objects[i].BoundingBox());

        return result;
    }

    public List<IHittable> Objects => _objects;
}