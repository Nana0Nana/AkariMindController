using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.TapNote")]
    internal abstract class TapNoteEx : TapNote
    {
        protected TapNoteCore _tapNoteCore = new TapNoteCore();
        public TapNoteCore TapNoteCore => _tapNoteCore as TapNoteCore;
    }
}
