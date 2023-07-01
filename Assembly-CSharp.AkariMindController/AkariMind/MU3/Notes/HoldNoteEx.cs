using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.HoldNote")]
    internal abstract class HoldNoteEx : HoldNote
    {
        protected TapNoteCore _tapNoteCore = new TapNoteCore();
        protected HoldNoteCore _holdNoteCore = new HoldNoteCore();

        public TapNoteCore TapNoteCore => _tapNoteCore;
        public HoldNoteCore HoldNoteCore => _holdNoteCore;
    }
}
