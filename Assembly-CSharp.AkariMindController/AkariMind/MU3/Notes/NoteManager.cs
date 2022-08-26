using MonoMod;
using MU3.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.NotesManager")]
    internal class NoteManagerEx : NotesManager
    {
        private NoteControlList _noteControlList;
        private float _curFrame;
        private NotesNodeCache _notesCache;

        public void ReapplyNotesVisible()
        {
            PatchLog.WriteLine($"call ReapplyNotesVisible()");

            for (int i = 0; i < _noteControlList.Count; i++)
            {
                NoteControl noteControl = _noteControlList[i];
                if (!noteControl.isPlay && !noteControl.isEnd && _curFrame >= noteControl.frameCreate)
                {
                    LinkedListNode<NotesBase> note = noteControl.createNotesBase(_notesCache);
                    addNotesBase(note);
                }
            }
        }
    }
}
