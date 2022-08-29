using MonoMod;
using MU3.Notes;
using MU3.Sound;
using MU3.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.NotesManager")]
    internal class NotesManagerEx : NotesManager
    {
        private static readonly int[] _seAnswerSound;

        private NoteControlList _noteControlList;
        private float _curFrame;
        private NotesNodeCache _notesCache;

        private float _frameNoteStart;
        private float _framePlayStart;

        public float getStartPlayFrame() => _framePlayStart;
        public float getStartNoteFrame() => _frameNoteStart;

        public void refreshNotesVisible()
        {
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

        public void playGuideSE(NotesManagerSE guideSEType = NotesManagerSE.GuideSE_Count)
        {
            PatchLog.WriteLine($"call playGuideSE() guideSEType = {guideSEType}");

            var j = (int)guideSEType;
            var num3 = 0f;
            if (j == 1 || j == 2)
            {
                num3 = GameOption.volGuide;
            }
            if (num3 > 0.01f)
            {
                Singleton<SoundManager>.instance.playVolume(_seAnswerSound[j], isLoop: false, num3);
            }
        }
    }
}
