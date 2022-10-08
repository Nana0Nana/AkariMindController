using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.ShellNoteCore")]
    internal class ShellNoteCoreEx : ShellNoteCore
    {
        private float widthShell;
        public float ShellWidth => widthShell;
    }
}
