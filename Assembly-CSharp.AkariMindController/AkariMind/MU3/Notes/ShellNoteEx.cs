using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.ShellNote")]
    internal class ShellNoteEx : ShellNote
    {
        protected ShellNoteCore _shellNoteCore = new ShellNoteCore();
        public ShellNoteCoreEx ShellNoteCore => _shellNoteCore as ShellNoteCoreEx;
    }
}
