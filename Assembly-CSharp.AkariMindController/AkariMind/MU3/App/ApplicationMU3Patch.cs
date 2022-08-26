using MonoMod;
using MU3.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AkariMindControllers.AkariMind.MU3.App
{
    [MonoModPatch("global::MU3.App.ApplicationMU3")]
    internal class ApplicationMU3Patch : ApplicationMU3
    {
        private extern void orig_onLogMessageReceived(string logString, string stackTrace, LogType type);

        private void onLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Assert || type == LogType.Exception || type == LogType.Error)
            {
                File.AppendAllText(PatchLog.FilePath, $"[{type}]" + logString + Environment.NewLine);
                File.AppendAllText(PatchLog.FilePath, stackTrace + Environment.NewLine);
            }

            orig_onLogMessageReceived(logString, stackTrace, type);
        }
    }
}
