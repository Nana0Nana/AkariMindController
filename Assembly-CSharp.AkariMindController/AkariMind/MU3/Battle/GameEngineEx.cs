using AkariMindControllers.AkariMind.MU3.Notes;
using MonoMod;
using MU3.Battle;
using MU3.Notes;
using MU3.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Battle
{
    [MonoModPatch("global::MU3.Battle.GameEngine")]
    internal class GameEngineEx : GameEngine
    {
        private NotesManager _notesManager;
        private bool dumpFailedAutoTargetDataOnce = false;

        public extern void orig_reset();
        public void reset()
        {
            orig_reset();
            dumpFailedAutoTargetDataOnce = false;
        }

        public extern void orig_killPlayer();
        public void killPlayer()
        {
            if (!dumpFailedAutoTargetDataOnce)
                (_notesManager as NotesManagerEx)?.dumpFailedAutoTargetData();
            orig_killPlayer();
            dumpFailedAutoTargetDataOnce = true;
        }
    }
}
