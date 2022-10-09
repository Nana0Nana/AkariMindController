using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.BeamNote")]
    internal class BeamNoteEx : BeamNote
    {
        protected BeamNoteCore _beamNoteCore = new BeamNoteCore();
        public BeamNoteCore ShellNoteCore => _beamNoteCore;
    }
}
