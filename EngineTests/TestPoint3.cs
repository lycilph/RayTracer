using Engine.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
    [TestClass]
    public class TestPoint3
    {
        [TestMethod]
        public void TestPointConstructor()
        {
            var p = new Point3(1, 2, 3);

            Assert.AreEqual(p.x, 1);
            Assert.AreEqual(p.y, 2);
            Assert.AreEqual(p.z, 3);

            Assert.AreEqual(p[Constants.X], p.x);
            Assert.AreEqual(p[Constants.Y], p.y);
            Assert.AreEqual(p[Constants.Z], p.z);
        }

        [TestMethod]
        public void TestPointAdd()
        {
            var p1 = new Point3(1, 2, 3);
            var p2 = new Point3(1, 1, 1);
            var p3 = p1 + p2;

            Assert.AreEqual(p3.x, 2);
            Assert.AreEqual(p3.y, 3);
            Assert.AreEqual(p3.z, 4);
        }

        [TestMethod]
        public void TestPointAddVector()
        {
            var p1 = new Point3(1, 2, 3);
            var v1 = new Vector3(1, 1, 1);
            var p2 = p1 + v1;

            Assert.AreEqual(p2.x, 2);
            Assert.AreEqual(p2.y, 3);
            Assert.AreEqual(p2.z, 4);
        }

        [TestMethod]
        public void TestPointMinus()
        {
            var p1 = new Point3(1, 2, 3);
            var p2 = new Point3(1, 1, 1);
            var v = p1 - p2;

            Assert.AreEqual(v.x, 0);
            Assert.AreEqual(v.y, 1);
            Assert.AreEqual(v.z, 2);
        }

        [TestMethod]
        public void TestPointMinusVector()
        {
            var p1 = new Point3(1, 2, 3);
            var v = new Vector3(1, 1, 1);
            var p2 = p1 - v;

            Assert.AreEqual(p2.x, 0);
            Assert.AreEqual(p2.y, 1);
            Assert.AreEqual(p2.z, 2);
        }

        [TestMethod]
        public void TestPointMultiplication()
        {
            var p1 = new Point3(1, 2, 3);
            var p2 = p1 * 5;
            var p3 = 5 * p1;

            Assert.AreEqual(p2.x, 5);
            Assert.AreEqual(p2.y, 10);
            Assert.AreEqual(p2.z, 15);

            Assert.AreEqual(p2.x, p3.x);
            Assert.AreEqual(p2.y, p3.y);
            Assert.AreEqual(p2.z, p3.z);
        }

        [TestMethod]
        public void TestPointDivision()
        {
            var p1 = new Point3(1, 2, 3);
            var p2 = p1 / 2;

            Assert.AreEqual(p2.x, 1.0 / 2);
            Assert.AreEqual(p2.y, 2.0 / 2);
            Assert.AreEqual(p2.z, 3.0 / 2);
        }
    }
}
