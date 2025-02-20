﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Macad.Common;
using Macad.Core;
using Macad.Core.Shapes;
using Macad.Interaction.Panels;
using Macad.Presentation;

namespace Macad.Interaction.Editors.Shapes
{
    /// <summary>
    /// Interaction logic for ChamferPropertyPanel.xaml
    /// </summary>
    public partial class ChamferPropertyPanel : PropertyPanel
    {
        public Chamfer Chamfer { get; private set; }

        //--------------------------------------------------------------------------------------------------

        public bool IsToolActive
        {
            get { return _IsToolActive; }
            set
            {
                if (_IsToolActive != value)
                {
                    _IsToolActive = value;
                    RaisePropertyChanged();
                }
            }
        }

        bool _IsToolActive;

        //--------------------------------------------------------------------------------------------------

        public ICommand StartToolCommand { get; private set; }

        void ExecuteStartTool()
        {
            if (IsToolActive)
            {
                (WorkspaceController.CurrentTool as EdgeModifierTool)?.Stop();
            }
            else
            {
                WorkspaceController.StartTool(new EdgeModifierTool(Chamfer));
            }
        }

        //--------------------------------------------------------------------------------------------------

        public ICommand SelectAllCommand { get; private set; }

        void ExecuteSelectAll()
        {
            Chamfer.AddAllEdges();
            CommmitChange();
        }

        //--------------------------------------------------------------------------------------------------

        public ICommand SelectNoneCommand { get; private set; }

        void ExecuteSelectNone()
        {
            Chamfer.RemoveAllEdges();
            CommmitChange();
        }

        //--------------------------------------------------------------------------------------------------

        public ICommand SwitchModeCommand { get; private set; }

        void ExecuteSwitchMode(Chamfer.ChamferModes mode)
        {
            if (Chamfer.Mode != mode)
            {
                Chamfer.Mode = mode;
                CommmitChange();
            }
        }

        //--------------------------------------------------------------------------------------------------

        public ICommand SwitchAngleSideCommand { get; private set; }

        void ExecuteSwitchAngleSideCommand()
        {
            if (Chamfer.Mode == Chamfer.ChamferModes.DistanceAngle)
            {
                Chamfer.ReverseOrientation = !Chamfer.ReverseOrientation;
                CommmitChange();
            }
        }

        //--------------------------------------------------------------------------------------------------

        void workspaceController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentTool")
            {
                IsToolActive = WorkspaceController.CurrentTool is EdgeModifierTool;
            }
        }

        //--------------------------------------------------------------------------------------------------

        public override void Initialize(BaseObject instance)
        {
            StartToolCommand = new RelayCommand(ExecuteStartTool);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            SelectNoneCommand = new RelayCommand(ExecuteSelectNone);
            SwitchModeCommand = new RelayCommand<Chamfer.ChamferModes>(ExecuteSwitchMode);
            SwitchAngleSideCommand = new RelayCommand(ExecuteSwitchAngleSideCommand);

            WorkspaceController.PropertyChanged += workspaceController_PropertyChanged;

            Chamfer = instance as Chamfer;

            InitializeComponent();
        }

        //--------------------------------------------------------------------------------------------------

        public override void Cleanup()
        {
            if (IsToolActive)
            {
                (WorkspaceController.CurrentTool as EdgeModifierTool)?.Stop();
            }
            WorkspaceController.PropertyChanged -= workspaceController_PropertyChanged;
        }

        //--------------------------------------------------------------------------------------------------
    }
}
