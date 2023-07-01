using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.FlickNote")]
    internal class FlickNoteEx : FlickNote
    {
        protected FlickNoteCore _flickNoteCore = new FlickNoteCore();
        public FlickNoteCoreEx FlickNoteCore => _flickNoteCore as FlickNoteCoreEx;
    }
}
