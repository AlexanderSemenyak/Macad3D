﻿using System;
using System.Windows;
using Macad.Common;
using Macad.Core;
using Macad.Core.Shapes;
using Macad.Presentation;

namespace Macad.Interaction.Editors.Shapes
{
    /// <summary>
    /// Interaction logic for ChamferPropertyPanel.xaml
    /// </summary>
    public partial class UnfoldSheetPropertyPanel : PropertyPanel
    {
        public UnfoldSheet UnfoldSheet { get; private set; }

        //--------------------------------------------------------------------------------------------------

        public override void Initialize(BaseObject instance)
        {
            UnfoldSheet = instance as UnfoldSheet;

            InitializeComponent();
        }

        //--------------------------------------------------------------------------------------------------

        public override void Cleanup()
        {
        }

        //--------------------------------------------------------------------------------------------------

    }
}
