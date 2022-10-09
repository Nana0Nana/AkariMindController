using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.BellNote_new")]
    internal abstract class BellNoteEx : NotesBase
    {
        protected BellNoteCore _bellNoteCore = new BellNoteCore();
        public BellNoteCoreEx BellNoteCore => _bellNoteCore as BellNoteCoreEx;
    }
}
