using System;
using Engine.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
    [TestClass]
    public class TestVector3
    {
        [TestMethod]
        public void TestVectorConstructor()
        {
            var v = new Vector3(1, 2, 3);

            Assert.AreEqual(v.x, 1);
            Assert.AreEqual(v.y, 2);
            Assert.AreEqual(v.z, 3);

            Assert.AreEqual(v[Constants.X], v.x);
            Assert.AreEqual(v[Constants.Y], v.y);
            Assert.AreEqual(v[Constants.Z], v.z);
        }

        [TestMethod]
        public void TestVectorAdd()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(1, 1, 1);
            var v3 = v1 + v2;

            Assert.AreEqual(v3.x, 2);
            Assert.AreEqual(v3.y, 3);
            Assert.AreEqual(v3.z, 4);
        }

        [TestMethod]
        public void TestVectorMinus()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(1, 1, 1);
            var v3 = v1 - v2;

            Assert.AreEqual(v3.x, 0);
            Assert.AreEqual(v3.y, 1);
            Assert.AreEqual(v3.z, 2);
        }

        [TestMethod]
        public void TestVectorUnaryMinus()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = -v1;

            Assert.AreEqual(v2.x, -1);
            Assert.AreEqual(v2.y, -2);
            Assert.AreEqual(v2.z, -3);
        }

        [TestMethod]
        public void TestVectorMultiplication()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = v1 * 5;
            var v3 = 5 * v1;

            Assert.AreEqual(v2.x, 5);
            Assert.AreEqual(v2.y, 10);
            Assert.AreEqual(v2.z, 15);

            Assert.AreEqual(v2.x, v3.x);
            Assert.AreEqual(v2.y, v3.y);
            Assert.AreEqual(v2.z, v3.z);
        }

        [TestMethod]
        public void TestVectorDivision()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = v1 / 2;

            Assert.AreEqual(v2.x, 1.0/2);
            Assert.AreEqual(v2.y, 2.0/2);
            Assert.AreEqual(v2.z, 3.0/2);
        }

        [TestMethod]
        public void TestNormalize()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = v1.Normalize();
            var v3 = Vector3.Normalize(v1);

            Assert.AreEqual(v2.x, 1.0/Math.Sqrt(14));
            Assert.AreEqual(v2.y, 2.0/Math.Sqrt(14));
            Assert.AreEqual(v2.z, 3.0/Math.Sqrt(14));

            Assert.AreEqual(v2.x, v3.x);
            Assert.AreEqual(v2.y, v3.y);
            Assert.AreEqual(v2.z, v3.z);
        }

        [TestMethod]
        public void TestDot()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(3, 2, 1);
            var d1 = v1.Dot(v2);
            var d2 = Vector3.Dot(v1, v2);

            Assert.AreEqual(d1, 10);
            Assert.AreEqual(d2, d1);
        }

        [TestMethod]
        public void TestAbsDot()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(-3, -2, -1);
            var d1 = v1.AbsDot(v2);
            var d2 = Vector3.AbsDot(v1, v2);

            Assert.AreEqual(d1, 10);
            Assert.AreEqual(d2, d1);
        }

        [TestMethod]
        public void TestCross()
        {
            var v1 = new Vector3(1, 0, 0);
            var v2 = new Vector3(0, 1, 0);
            var v3 = v1.Cross(v2);
            var v4 = Vector3.Cross(v1, v2);

            Assert.AreEqual(v3.x, 0);
            Assert.AreEqual(v3.y, 0);
            Assert.AreEqual(v3.z, 1);

            Assert.AreEqual(v4.x, v3.x);
            Assert.AreEqual(v4.y, v3.y);
            Assert.AreEqual(v4.z, v3.z);
        }
    }
}
