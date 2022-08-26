using System;
using AkiraMindController.Communication;
using AkiraMindController.Communication.Connectors;
using MonoMod;
using MU3.AM;
using MU3.Util;
using UnityEngine;

namespace AkariMindControllers
{
    [MonoModPatch("global::MU3.AM.AMManager")]
    public class MainLoader : SingletonStateMachine<AMManager, AMManager.EState>
    {
        public extern void orig_initialize();

        public void initialize()
        {
            orig_initialize();

            PatchLog.WriteLine("After orig_initialize().");
            Controller.Init();
            PatchLog.WriteLine($"QualitySettings.vSyncCount = {QualitySettings.vSyncCount}");
            PatchLog.WriteLine($"Screen.currentResolution.refreshRate = {Screen.currentResolution.refreshRate}");
            
            QualitySettings.vSyncCount = 0;
            //Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen, 120);
        }
    }
}