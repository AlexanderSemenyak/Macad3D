﻿using System.IO;
using Macad.Test.Utils;
using Macad.Core;
using Macad.Core.Shapes;
using Macad.Interaction;
using Macad.Occt;
using NUnit.Framework;
using Macad.Interaction.Editors.Shapes;
using System.Windows.Input;
using System;

namespace Macad.Test.Unit.Interaction.Multiply;

[TestFixture]
public class CircularArrayEditorTests
{
    const string _BasePath = @"Interaction\Multiply\CircularArray";

    //--------------------------------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        Context.InitWithView(500);
        Context.Current.Viewport.V3dView.SetBackgroundColor(Quantity_NameOfColor.BLACK.ToColor());
    }

    [TearDown]
    public void TearDown()
    {
        Context.Current.Deinit();
    }

    //--------------------------------------------------------------------------------------------------
        
    [Test]
    public void CreateFromSketch()
    {
        var ctx = Context.Current;
        var sketch = TestSketchGenerator.CreateSketch(TestSketchGenerator.SketchType.SimpleAsymmetric, true);
        ctx.ViewportController.ZoomFitAll();
        ctx.WorkspaceController.Selection.SelectEntity(sketch.Body);

        var tool = new CreateCircularArrayTool(sketch.Body);
        ctx.WorkspaceController.StartTool(tool);

        Assert.Multiple(() =>
        {
            Assert.IsInstanceOf<CircularArray>(sketch.Body.Shape);
            Assert.IsNull(ctx.WorkspaceController.CurrentTool);
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "CreateFromSketch01"));
        });
    }

    //--------------------------------------------------------------------------------------------------
    
    [Test]
    public void CreateFromSolid()
    {
        var ctx = Context.Current;
        var imprint = TestGeomGenerator.CreateImprint();
        ctx.ViewportController.ZoomFitAll();
        ctx.WorkspaceController.Selection.SelectEntity(imprint.Body);

        var tool = new CreateCircularArrayTool(imprint.Body);
        ctx.WorkspaceController.StartTool(tool);

        Assert.Multiple(() =>
        {
            var circularArray = imprint.Body.Shape as CircularArray;
            Assert.IsNotNull(circularArray);
            Assert.AreEqual(tool, ctx.WorkspaceController.CurrentTool);
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "CreateFromSolid01"));

            ctx.MoveTo(178, 262);
            Assert.AreEqual(CircularArray.PlaneType.ZX, circularArray.Plane);
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "CreateFromSolid02"));

            ctx.MoveTo(239, 124);
            Assert.AreEqual(CircularArray.PlaneType.YZ, circularArray.Plane);
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "CreateFromSolid03"));

            ctx.ClickAt(337, 291);
            Assert.AreEqual(CircularArray.PlaneType.XY, circularArray.Plane);
            Assert.IsNull(ctx.WorkspaceController.CurrentTool);
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "CreateFromSolid04"));
        });
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void EditorIdle()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.ViewportController.ZoomFitAll();
        ctx.WorkspaceController.StartEditor(array);

        Assert.Multiple(() =>
        {
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle01"));

            // Cleanup
            ctx.WorkspaceController.StopEditor();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle99"));
        });
    }

    //--------------------------------------------------------------------------------------------------
    
    [Test]
    public void EditorIdleSketch()
    {
        var ctx = Context.Current;
        var array = _CreateSketchArray();
        ctx.ViewportController.ZoomFitAll();
        ctx.WorkspaceController.StartEditor(array);

        Assert.Multiple(() =>
        {
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdleSketch01"));

            // Cleanup
            ctx.WorkspaceController.StopEditor();
        });
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void EditorStartStopTools()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.ViewportController.ZoomFitAll();
        ctx.WorkspaceController.StartEditor(array);

        Assert.Multiple(() =>
        {
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle01"));
            ctx.WorkspaceController.CurrentEditor.StopTools();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle02"));
            array.RaiseShapeChanged();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle02"));
            ctx.WorkspaceController.CurrentEditor.StartTools();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle01"));

            // Cleanup
            ctx.WorkspaceController.StopEditor();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle99"));
        });
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void EditorIdleSolid()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.ViewportController.ZoomFitAll();
        ctx.WorkspaceController.Selection.SelectEntity(array.Body);

        var editor = Editor.CreateEditor(array);
        editor.Start();

        Assert.Multiple(() =>
        {
            array.Plane = CircularArray.PlaneType.XY;
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdleSolid_XY"));
            array.Plane = CircularArray.PlaneType.YZ;
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdleSolid_YZ"));
            array.Plane = CircularArray.PlaneType.ZX;
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdleSolid_ZX"));

            // Cleanup
            editor.Stop();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdleSolid_Clean"));
        });
    }
        
    //--------------------------------------------------------------------------------------------------

    [Test]
    public void EditorIdleNegativeValues()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.ViewportController.ZoomFitAll();
        ctx.WorkspaceController.Selection.SelectEntity(array.Body);

        var editor = Editor.CreateEditor(array);
        editor.Start();

        Assert.Multiple(() =>
        {
            array.Plane = CircularArray.PlaneType.XY;
            array.Radius *= -1;
            ctx.ViewportController.ZoomFitAll();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "EditorIdle_NegRadius"));

            // Cleanup
            editor.Stop();
        });
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void LiveRadius()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.WorkspaceController.StartEditor(array);

        var oldRadius = array.Radius;
        ctx.ViewportController.ZoomFitAll();

        Assert.Multiple(() =>
        {
            ctx.MoveTo(283, 232);
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadius01"));

            ctx.ViewportController.MouseDown();
            ctx.MoveTo(316, 186);
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadius02"));
            
            ctx.ViewportController.MouseUp();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadius03"));
            Assert.Greater(array.Radius, oldRadius);

            Assert.IsNull(ctx.TestHudManager.HintMessageOwner);
            Assert.IsEmpty(ctx.TestHudManager.HudElements);

            // Cleanup
            ctx.WorkspaceController.StopEditor();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadius99"));
        });
    }
    
    //--------------------------------------------------------------------------------------------------

    [Test]
    public void LiveRadiusClamp()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.WorkspaceController.StartEditor(array);
        ctx.WorkspaceController.Workspace.GridStep = 10.0;
        ctx.ViewportController.ZoomFitAll();

        Assert.Multiple(() =>
        {
            ctx.MoveTo(283, 232);
            ctx.ViewportController.MouseDown();
            ctx.MoveTo(316, 186, ModifierKeys.Control);
            ctx.ViewportController.MouseUp();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadiusClamp01"));
            Assert.AreEqual(90.0, array.Radius);

            // Cleanup
            ctx.WorkspaceController.StopEditor();
        });
    }
    
    //--------------------------------------------------------------------------------------------------

    [Test]
    public void LiveRadiusRotated()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        array.OriginalAngle = 30;
        ctx.WorkspaceController.StartEditor(array);
        ctx.ViewportController.ZoomFitAll();

        Assert.Multiple(() =>
        {
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadiusRotated01"));
            ctx.MoveTo(270, 234);
            ctx.ViewportController.MouseDown();
            ctx.MoveTo(284, 202);
            ctx.ViewportController.MouseUp();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadiusRotated02"));

            // Cleanup
            ctx.WorkspaceController.StopEditor();
        });
    }
    
    //--------------------------------------------------------------------------------------------------

    [Test]
    public void LiveRadiusUndo()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.WorkspaceController.StartEditor(array);
        ctx.UndoHandler.Commit();
        Assert.AreEqual(1, ctx.UndoHandler.UndoStack.Count);

        var oldRadius = array.Radius;
        ctx.ViewportController.ZoomFitAll();
        ctx.MoveTo(283, 232);
        ctx.ViewportController.MouseDown();
        ctx.MoveTo(316, 186);
        ctx.ViewportController.MouseUp();
        ctx.WorkspaceController.StopEditor();

        Assert.Greater(array.Radius, oldRadius);
        Assert.AreEqual(2, ctx.UndoHandler.UndoStack.Count);

        ctx.UndoHandler.DoUndo(1);

        Assert.AreEqual(oldRadius, array.Radius);
        Assert.AreEqual(1, ctx.UndoHandler.UndoStack.Count);
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void LiveOriginalAngle()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.WorkspaceController.StartEditor(array);

        var oldAngle = array.OriginalAngle;
        ctx.ViewportController.ZoomFitAll();

        Assert.Multiple(() =>
        {
            ctx.MoveTo(284, 254);
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveOriginalAngle01"));
            
            ctx.ViewportController.MouseDown();
            ctx.MoveTo(324, 282);
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveOriginalAngle02"));
            
            ctx.ViewportController.MouseUp();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveOriginalAngle03"));
            Assert.Greater(array.Radius, oldAngle);

            Assert.IsNull(ctx.TestHudManager.HintMessageOwner);
            Assert.IsEmpty(ctx.TestHudManager.HudElements);

            // Cleanup
            ctx.WorkspaceController.StopEditor();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveOriginalAngle99"));
        });
    }

    //--------------------------------------------------------------------------------------------------

    [Test]
    public void LiveOriginalAngleClamp()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.WorkspaceController.StartEditor(array);
        ctx.ViewportController.ZoomFitAll();

        Assert.Multiple(() =>
        {
            ctx.MoveTo(284, 254);
            ctx.ViewportController.MouseDown();
            ctx.MoveTo(324, 282, ModifierKeys.Control);
            ctx.ViewportController.MouseUp();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveOriginalAngleClamp01"));
            Assert.AreEqual(-10.0, array.OriginalAngle);

            // Cleanup
            ctx.WorkspaceController.StopEditor();
        });
    }

    //--------------------------------------------------------------------------------------------------
    
    [Test]
    public void LiveOriginalAngleUndo()
    {
        var ctx = Context.Current;
        var array = _CreateSolidArray();
        ctx.WorkspaceController.StartEditor(array);
        ctx.UndoHandler.Commit();
        Assert.AreEqual(1, ctx.UndoHandler.UndoStack.Count);

        var oldAngle = array.OriginalAngle;
        ctx.ViewportController.ZoomFitAll();
        ctx.MoveTo(284, 254);
        ctx.ViewportController.MouseDown();
        ctx.MoveTo(324, 282);
        ctx.ViewportController.MouseUp();
        ctx.WorkspaceController.StopEditor();

        Assert.Less(array.OriginalAngle, oldAngle);
        Assert.AreEqual(2, ctx.UndoHandler.UndoStack.Count);

        ctx.UndoHandler.DoUndo(1);

        Assert.AreEqual(oldAngle, array.OriginalAngle);
        Assert.AreEqual(1, ctx.UndoHandler.UndoStack.Count);
    }
    
    //--------------------------------------------------------------------------------------------------

    [Test]
    public void LiveRadiusSketchTransformedPlane()
    {
        var ctx = Context.Current;
        var section = TestGeomGenerator.CreateCrossSection();
        var circularArray = CircularArray.Create(section.Body);
        circularArray.Radius = 50;
        ctx.WorkspaceController.StartEditor(circularArray);

        var oldRadius = circularArray.Radius;
        ctx.ViewportController.ZoomFitAll();

        Assert.Multiple(() =>
        {
            ctx.MoveTo(271, 260);
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadiusSketchTransformedPlane01"));
            ctx.ViewportController.MouseDown();
            ctx.MoveTo(310, 277);
            ctx.ViewportController.MouseUp();
            AssertHelper.IsSameViewport(Path.Combine(_BasePath, "LiveRadiusSketchTransformedPlane02"));
            Assert.Greater(circularArray.Radius, oldRadius);

            // Cleanup
            ctx.WorkspaceController.StopEditor();
        });
    }

    //--------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------

    CircularArray _CreateSolidArray()
    {
        var solid = TestGeomGenerator.CreateImprint();
        var array = CircularArray.Create(solid.Body);
        array.Quantity = 4;
        array.Radius = 50;
        array.OriginalAngle = 20;
        array.Range = 220;
        array.Alignment = CircularArray.AlignmentMode.Center;
        Assume.That(array.Make(Shape.MakeFlags.None));

        return array;
    }

    //--------------------------------------------------------------------------------------------------

    CircularArray _CreateSketchArray()
    {
        var sketch = TestSketchGenerator.CreateSketch(createBody: true);
        var array = CircularArray.Create(sketch.Body);
        array.Quantity = 4;
        array.Radius = 50;
        array.OriginalAngle = 20;
        array.Range = 220;
        array.Alignment = CircularArray.AlignmentMode.Center;
        Assume.That(array.Make(Shape.MakeFlags.None));
        return array;
    }
}