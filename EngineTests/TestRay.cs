using Engine.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
    [TestClass]
    public class TestRay
    {
        [TestMethod]
        public void TestRayIndexer()
        {
            var r = new Ray(new Point3(0, 0, 0), new Vector3(1,2,3));
            var p = r[5];

            Assert.AreEqual(p.x, 5);
            Assert.AreEqual(p.y, 10);
            Assert.AreEqual(p.z, 15);
        }
    }
}
