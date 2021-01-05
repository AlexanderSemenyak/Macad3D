﻿using System.IO;
using System.Windows;
using Macad.Test.Utils;
using Macad.Core;
using Macad.Core.Shapes;
using Macad.Core.Topology;
using Macad.Interaction;
using Macad.Occt;
using NUnit.Framework;

namespace Macad.Test.Unit.Interaction.Infrastructure
{
    [TestFixture]
    public class SelectionTests
    {
        const string _BasePath = @"Interaction\Infrastructure\Selection";

        //--------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            Context.InitWithView(500);
        }

        [TearDown]
        public void TearDown()
        {
            Context.Current.Deinit();
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void SingleSelection()
        {
            var ctx = Context.Current;
            var sel = ctx.WorkspaceController.Selection;

            var body1 = TestData.GetBodyFromBRep(@"SourceData\BRep\ImprintRingFace.brep");
            body1.Position = new Pnt(-15, 0, 0);
            var body2 = TestData.GetBodyFromBRep(@"SourceData\BRep\ImprintRingFace.brep");
            body2.Position = new Pnt(15, 0, 0);
            ctx.ViewportController.ZoomFitAll();

            Assert.Multiple(() =>
            {
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SingleSelection01"));

                // Select first
                ctx.ViewportController.MouseMove(new Point(350, 180));
                ctx.ViewportController.MouseDown();
                ctx.ViewportController.MouseUp(false);
                ctx.ViewportController.MouseMove(new Point(0, 0));
                Assert.That(sel.SelectedEntities.Count, Is.EqualTo(1));
                Assert.That(sel.SelectedEntities[0], Is.EqualTo(body1));
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SingleSelection02"));

                // Select second
                ctx.ViewportController.MouseMove(new Point(150, 290));
                ctx.ViewportController.MouseDown();
                ctx.ViewportController.MouseUp(false);
                ctx.ViewportController.MouseMove(new Point(0, 0));
                Assert.That(sel.SelectedEntities.Count, Is.EqualTo(1));
                Assert.That(sel.SelectedEntities[0], Is.EqualTo(body2));
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SingleSelection03"));

                // Unselect
                ctx.ViewportController.MouseDown();
                ctx.ViewportController.MouseUp(false);
                Assert.That(sel.SelectedEntities.Count, Is.EqualTo(0));
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SingleSelection04"));
            });
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void MultiSelection()
        {
            var ctx = Context.Current;
            var sel = ctx.WorkspaceController.Selection;

            var body1 = TestData.GetBodyFromBRep(@"SourceData\BRep\ImprintRingFace.brep");
            body1.Position = new Pnt(-15, 0, 0);
            var body2 = TestData.GetBodyFromBRep(@"SourceData\BRep\ImprintRingFace.brep");
            body2.Position = new Pnt(15, 0, 0);
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SingleSelection01"));

            // Select both
            ctx.ViewportController.MouseMove(new Point(350, 180));
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.MouseUp(false);
            ctx.ViewportController.MouseMove(new Point(150, 290));
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.MouseUp(true);
            ctx.ViewportController.MouseMove(new Point(0, 0));
            Assert.That(sel.SelectedEntities.Count, Is.EqualTo(2));
            Assert.That(sel.SelectedEntities[0], Is.EqualTo(body1));
            Assert.That(sel.SelectedEntities[1], Is.EqualTo(body2));
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "MultiSelection02"));

            // Select single again
            ctx.ViewportController.MouseMove(new Point(150, 290));
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.MouseUp(false);
            ctx.ViewportController.MouseMove(new Point(0, 0));
            Assert.That(sel.SelectedEntities.Count, Is.EqualTo(1));
            Assert.That(sel.SelectedEntities[0], Is.EqualTo(body2));
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SingleSelection03"));

            // Unselect
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.MouseUp(false);
            Assert.That(sel.SelectedEntities.Count, Is.EqualTo(0));
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SingleSelection04"));
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void SelectionRestoreAfterBrepUpdate()
        {
            var ctx = Context.Current;
            var sel = ctx.WorkspaceController.Selection;

            var body1 = TestData.GetBodyFromBRep(@"SourceData\BRep\ImprintRingFace.brep");
            ctx.ViewportController.ZoomFitAll();

            // Select
            ctx.ViewportController.MouseMove(new Point(250, 250));
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.MouseUp(false);
            ctx.ViewportController.MouseMove(new Point(0, 0));
            Assert.That(sel.SelectedEntities.Count, Is.EqualTo(1));
            Assert.That(sel.SelectedEntities[0], Is.EqualTo(body1));
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SelectionRestoreAfterBrepUpdate1"));

            // Update
            Chamfer.Create(body1, new[] {new SubshapeReference(SubshapeType.Edge, body1.Shape.Guid, 14)});
            Assert.That(sel.SelectedEntities.Count, Is.EqualTo(1));
            Assert.That(sel.SelectedEntities[0], Is.EqualTo(body1));
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "SelectionRestoreAfterBrepUpdate2"));
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void RubberbandRectangleSelection()
        {
            var ctx = Context.Current;
            var sel = ctx.WorkspaceController.Selection;

            var body1 = Body.Create(Box.Create(10, 10, 10));
            body1.Position = new Pnt(-15, 0, 0);
            var body2 = Body.Create(Cylinder.Create(6, 8));
            body2.Position = new Pnt(15, 0, 0);
            var body3 = Body.Create(Sphere.Create(8));
            body2.Position = new Pnt(0, 15, 0);
            ctx.ViewportController.ZoomFitAll();

            // Select two of three
            ctx.MoveTo(170, 30);
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.StartRubberbandSelection(ViewportController.RubberbandSelectionMode.Rectangle);
            ctx.MoveTo(400, 400);
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "RubberbandSelection01"));

            ctx.ViewportController.MouseUp(false);
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "RubberbandSelection02"));
            Assert.AreEqual(2, sel.SelectedEntities.Count);

            // Additional selection
            ctx.MoveTo(30, 120);
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.StartRubberbandSelection(ViewportController.RubberbandSelectionMode.Rectangle);
            ctx.MoveTo(290, 420);
            ctx.ViewportController.MouseUp(true);
            Assert.AreEqual(3, sel.SelectedEntities.Count);

            // Reduce selection
            ctx.MoveTo(30, 120);
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.StartRubberbandSelection(ViewportController.RubberbandSelectionMode.Rectangle);
            ctx.MoveTo(290, 420);
            ctx.ViewportController.MouseUp(false);
            Assert.AreEqual(1, sel.SelectedEntities.Count);
            
            // Empty selection
            ctx.MoveTo(10, 10);
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.StartRubberbandSelection(ViewportController.RubberbandSelectionMode.Rectangle);
            ctx.MoveTo(100, 100);
            ctx.ViewportController.MouseUp(false);
            Assert.AreEqual(0, sel.SelectedEntities.Count);
        }

        //--------------------------------------------------------------------------------------------------

        [Test]
        public void RubberbandFreehandSelection()
        {
            var ctx = Context.Current;
            var sel = ctx.WorkspaceController.Selection;

            var body1 = Body.Create(Box.Create(10, 10, 10));
            body1.Position = new Pnt(-15, 0, 0);
            var body2 = Body.Create(Cylinder.Create(6, 8));
            body2.Position = new Pnt(15, 0, 0);
            var body3 = Body.Create(Cylinder.Create(4, 6));
            body2.Position = new Pnt(0, 15, 0);
            ctx.ViewportController.ZoomFitAll();

            // Select two of three
            ctx.MoveTo(300, 50);
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.StartRubberbandSelection(ViewportController.RubberbandSelectionMode.Freehand);
            ctx.MoveTo(395, 120);
            ctx.MoveTo(378, 430);
            ctx.MoveTo(188, 446);
            ctx.MoveTo(130, 250);
            ctx.MoveTo(150, 120);
            
            Assert.Multiple(() =>
            {
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "RubberbandSelection11"));

                ctx.ViewportController.MouseUp(false);
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "RubberbandSelection12"));
                Assert.AreEqual(2, sel.SelectedEntities.Count);
            });
        }
        
        //--------------------------------------------------------------------------------------------------

        [Test]
        public void RubberbandFreehandOverlapSelection()
        {
            var ctx = Context.Current;
            var sel = ctx.WorkspaceController.Selection;

            var body1 = Body.Create(Box.Create(10, 10, 10));
            body1.Position = new Pnt(-15, 0, 0);
            var body2 = Body.Create(Cylinder.Create(6, 8));
            body2.Position = new Pnt(15, 0, 0);
            var body3 = Body.Create(Cylinder.Create(4, 6));
            body2.Position = new Pnt(0, 15, 0);
            ctx.ViewportController.ZoomFitAll();

            // Select two of three
            ctx.MoveTo(300, 50);
            ctx.ViewportController.MouseDown();
            ctx.ViewportController.StartRubberbandSelection(ViewportController.RubberbandSelectionMode.Freehand);
            ctx.MoveTo(395, 120);
            ctx.MoveTo(392, 237);
            ctx.MoveTo(317, 253);
            ctx.MoveTo(288, 148);
            //ctx.MoveTo(190, 139);
            Assert.Multiple(() =>
            {
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "RubberbandSelection21"));

                ctx.ViewportController.MouseUp(false);
                AssertHelper.IsSameViewport(Path.Combine(_BasePath, "RubberbandSelection22"));
                Assert.AreEqual(0, sel.SelectedEntities.Count);
            });
        }
    }
}