﻿using System.Linq;
using Macad.Test.Utils;
using Macad.Core;
using Macad.Occt;
using Macad.Occt.Helper;
using NUnit.Framework;

namespace Macad.Test.Unit.Exchange
{
    [TestFixture]
    public class BrepTests
    {
        [Test]
        public void AsciiReadWrite()
        {
            var originalBytes = TestData.GetTestData(@"SourceData\Brep\ImprintRingFace.brep");
            Assume.That(originalBytes, Is.Not.Null);

            // Read in
            var originalShape = BRepExchange.ReadASCII(originalBytes);
            Assert.IsNotNull(originalShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, originalShape.ShapeType());

            // Write out
            var writtenBytes = BRepExchange.WriteASCII(originalShape, false);
            Assert.IsNotNull(writtenBytes);
            Assert.AreEqual(4900, writtenBytes.Length, 50); // due to some slight differences (e.g. +/-0)

            // Re-read in
            var rereadShape = BRepExchange.ReadASCII(writtenBytes);
            Assert.IsNotNull(rereadShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, rereadShape.ShapeType());
            Assert.IsFalse(_HasTriangulation(rereadShape), "HasTriangulation");

            Assert.IsTrue(ModelCompare.CompareShape(rereadShape, @"SourceData\Brep\ImprintRingFace"));
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void BinaryReadWrite()
        {
            var originalBytes = TestData.GetTestData(@"SourceData\Brep\ImprintRingFace.brep");
            Assume.That(originalBytes, Is.Not.Null);

            // Read in as ASCII
            var originalShape = BRepExchange.ReadASCII(originalBytes);
            Assert.IsNotNull(originalShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, originalShape.ShapeType());

            // Write out
            var writtenBytes = BRepExchange.WriteBinary(originalShape, false);
            Assert.IsNotNull(writtenBytes);
            Assert.AreEqual(7222, writtenBytes.Length);

            // Re-read in
            var rereadShape = BRepExchange.ReadBinary(writtenBytes);
            Assert.IsNotNull(rereadShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, rereadShape.ShapeType());
            Assert.IsFalse(_HasTriangulation(rereadShape), "HasTriangulation");

            Assert.IsTrue(ModelCompare.CompareShape(rereadShape, @"SourceData\Brep\ImprintRingFace"));
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void AsciiTriangulation()
        {
            var originalBytes = TestData.GetTestData(@"SourceData\Brep\Motor-c.brep");
            Assume.That(originalBytes, Is.Not.Null);

            // Read in
            var originalShape = BRepExchange.ReadASCII(originalBytes);
            Assert.IsNotNull(originalShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, originalShape.ShapeType());

            // Write out with triangulation
            var writtenBytes = BRepExchange.WriteASCII(originalShape, true);
            Assert.IsNotNull(writtenBytes);
            Assert.AreEqual(2584000, writtenBytes.Length, 1000); // due to some slight differences (e.g. +/-0)

            // Re-read in with triangulation
            var rereadShape = BRepExchange.ReadASCII(writtenBytes);
            Assert.IsNotNull(rereadShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, rereadShape.ShapeType());
            Assert.IsTrue(_HasTriangulation(rereadShape), "HasTriangulation");
            Assert.IsTrue(ModelCompare.CompareShape(rereadShape, @"SourceData\Brep\Motor-c"));

            // Write out w/o triangulation
            writtenBytes = BRepExchange.WriteASCII(originalShape, false);
            Assert.IsNotNull(writtenBytes);
            Assert.AreEqual(1118000, writtenBytes.Length, 1000); // due to some slight differences (e.g. +/-0)

            // Re-read in w/o triangulation
            rereadShape = BRepExchange.ReadASCII(writtenBytes);
            Assert.IsNotNull(rereadShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, rereadShape.ShapeType());
            Assert.IsFalse(_HasTriangulation(rereadShape), "HasTriangulation");
            Assert.IsTrue(ModelCompare.CompareShape(rereadShape, @"SourceData\Brep\Motor-c"));
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void BinaryTriangulation()
        {
            var originalBytes = TestData.GetTestData(@"SourceData\Brep\Motor-c.brep");
            Assume.That(originalBytes, Is.Not.Null);

            // Read in
            var originalShape = BRepExchange.ReadASCII(originalBytes);
            Assert.IsNotNull(originalShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, originalShape.ShapeType());

            // Write out with triangulation
            var writtenBytes = BRepExchange.WriteBinary(originalShape, true);
            Assert.IsNotNull(writtenBytes);
            Assert.AreEqual(1624845, writtenBytes.Length);

            // Re-read in with triangulation
            var rereadShape = BRepExchange.ReadBinary(writtenBytes);
            Assert.IsNotNull(rereadShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, rereadShape.ShapeType());
            Assert.IsTrue(_HasTriangulation(rereadShape), "HasTriangulation");
            Assert.IsTrue(ModelCompare.CompareShape(rereadShape, @"SourceData\Brep\Motor-c"));

            // Write out w/o triangulation
            writtenBytes = BRepExchange.WriteBinary(originalShape, false);
            Assert.IsNotNull(writtenBytes);
            Assert.AreEqual(665759, writtenBytes.Length);

            // Re-read in w/o triangulation
            rereadShape = BRepExchange.ReadBinary(writtenBytes);
            Assert.IsNotNull(rereadShape);
            Assert.AreEqual(TopAbs_ShapeEnum.TopAbs_COMPOUND, rereadShape.ShapeType());
            Assert.IsFalse(_HasTriangulation(rereadShape), "HasTriangulation");
            Assert.IsTrue(ModelCompare.CompareShape(rereadShape, @"SourceData\Brep\Motor-c"));
        }

        //--------------------------------------------------------------------------------------------------

        bool _HasTriangulation(TopoDS_Shape shape)
        {
            var faces = shape.Faces();
            return faces.Any(face => BRepTools.Triangulation(face, Precision.Infinite()));
        }

        //--------------------------------------------------------------------------------------------------

    }
}