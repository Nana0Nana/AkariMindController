using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind
{
    [MonoModPatch("global::GameDeviceManager")]
    internal class GameDeviceManagerEx : GameDeviceManager
    {
        private float[] _faderLog = Enumerable.Repeat(0f, 30).ToArray();

        public void setFader(float value = 0, int log = 0)
        {
            if (log >= 30)
                return;

            _faderLog[log] = value;
        }
    }
}
