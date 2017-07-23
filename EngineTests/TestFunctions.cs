using System;
using Engine.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
    [TestClass]
    public class TestFunctions
    {
        [TestMethod]
        public void TestQuadratic()
        {
            bool b = Functions.Quadratic(1.0, -2.0, -3.0, out double t0, out double t1);

            Assert.IsTrue(b);
            Assert.AreEqual(t0, -1);
            Assert.AreEqual(t1, 3);
        }
    }
}
