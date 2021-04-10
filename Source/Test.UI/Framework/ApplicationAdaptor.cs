﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Macad.Common;
using NUnit.Framework;

namespace Macad.Test.UI.Framework
{
    public class ApplicationAdaptor
    {
        #region Properties

        public FlaUI.Core.Application Application { get; private set; }

        //--------------------------------------------------------------------------------------------------

        #endregion

        #region Init / Deinit

        public ApplicationAdaptor()
        {
        }

        //--------------------------------------------------------------------------------------------------

        public void Init(bool enableWelcomeDialog, string filepath=null)
        {
            // Set up paths
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var applicationPath = Path.Combine(applicationDirectory, "Macad.exe");
            var applicationArguments = "-sandbox";
            if (!enableWelcomeDialog)
            {
                applicationArguments += " -nowelcome";
            }

            if (!filepath.IsNullOrEmpty())
            {
                applicationArguments += " \"" + filepath + "\"";
            }

            // Start the app
            var processStartInfo = new ProcessStartInfo
            {
                FileName = applicationPath,
                WorkingDirectory = applicationDirectory,
                Arguments = applicationArguments
            };

            Application = FlaUI.Core.Application.Launch(processStartInfo);
            Assume.That(Application, Is.Not.Null);
            Application.WaitWhileMainHandleIsMissing();
        }

        //--------------------------------------------------------------------------------------------------

        public void Cleanup()
        {
            Application?.Kill();
        }

        #endregion

        public Window FindWindow(Func<Window,bool> predicateFunc)
        {
            using (var automation = new UIA3Automation())
            {
                return Application.GetAllTopLevelWindows(automation).FirstOrDefault(predicateFunc);
            }
        }
    }
}