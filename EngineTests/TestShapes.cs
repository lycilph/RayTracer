using System;
using Engine.Geometry;
using Engine.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
    [TestClass]
    public class TestShapes
    {
        [TestMethod]
        public void TestSphere()
        {
            Shape sphere = new Sphere(3);
            var ray = new Ray(new Point3(0, 0, -10), new Vector3(0, 0, 1));

            var intersect = sphere.Intersect(ray, out double t);

            Assert.IsTrue(intersect);
            Assert.AreEqual(t, 7);
        }
    }
}
