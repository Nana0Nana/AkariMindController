using AkariMindControllers.Utils;
using MonoMod;
using MU3.Battle;
using MU3.Notes;
using MU3.Sound;
using MU3.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.NotesManager")]
    internal class NotesManagerEx : NotesManager
    {
        private NotesList _pNotesList = new NotesList();
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

            var soundID = (int)guideSEType;
            var vol = 1f;
            if (soundID == 1 || soundID == 2)
                vol = GameOption.volGuide;
            if (vol > 0.01f)
                Singleton<SoundManager>.instance.playVolume(_seAnswerSound[soundID], isLoop: false, vol);
        }

        bool isEnableAutoPlay = false;

        public extern bool orig_isAutoPlay();
        private extern void orig_updateFader();

        public bool isAutoPlay()
        {
            if (orig_isAutoPlay())
                return true;

            return isEnableAutoPlay;
        }

        private extern void orig_createFieldState(float frame, ref FieldState fieldState);
        private void createFieldState(float frame, ref FieldState fieldState) => orig_createFieldState(frame, ref fieldState);

        public float fakeButtomMsec = 500;

        private float calculateAutoPlayFader()
        {
            var getNextShellNotes = _pNotesList
                .OfType<ShellNoteEx>()
                .Where(x => x.getShellType() != Shells.MAX)
                .Select(x => x.ShellNoteCore)
                .GroupBy(x => x.param.frameHit)
                .Where(x => x.Key > _curFrame)
                .OrderBy(x => x.Key)
                .FirstOrDefault();

            if (getNextShellNotes is null)
                return fieldState.area.posInC;

            var shellFrame = getNextShellNotes.Key;
            var shellFieldState = new FieldState()
            {
                area = new FieldObject.Area()
            };

            createFieldState(shellFrame, ref shellFieldState);

            //摇杆可移动区域
            var movableRange = new ValueRange(shellFieldState.area.posCenterL, shellFieldState.area.posCenterR);
            //中弹区域
            var dangeRanges = ValueRange.Union(getNextShellNotes.Select(x =>
            {
                var width = x.ShellWidth;
                return new ValueRange(x.param.placeHit - width * 0.5f, x.param.placeHit + width * 0.5f);
            }));
            //安全可移动区域
            var safeRanges = ValueRange.Except(movableRange, dangeRanges);

            //选择最近的一个点去插值

            var currentFaderPlace = SingletonMonoBehaviour<GameEngine>.instance.gameDeviceManager.getFader();
            var pickSafePlace = safeRanges.OrderByDescending(x => x.Max - x.Min).Select(x => (x.Max + x.Min) / 2).FirstOrDefault();
            var fakeButtomPlace = 10f * (currentFaderPlace > pickSafePlace ? 1 : -1);
            fakeButtomPlace = Math.Max(fieldState.area.posCenterL, Math.Min(fieldState.area.posCenterR, fakeButtomPlace));
            var fakeButtomFrame = Math.Min(0, _curFrame - fakeButtomMsec / 16.6666f);

            var adjustPlace = MathUtils.CalculateXFromTwoPointFormFormula(_curFrame, fakeButtomPlace, fakeButtomFrame, pickSafePlace, shellFrame);
            adjustPlace = Math.Max(fieldState.area.posCenterL, Math.Min(fieldState.area.posCenterR, adjustPlace));

            return (float)adjustPlace;
        }

        private void updateFader()
        {
            var gameObj = SingletonMonoBehaviour<GameEngine>.instance.gameDeviceManager as GameDeviceManagerEx;

            if (isAutoPlay() && isPlaying)
                gameObj?.setFader(calculateAutoPlayFader());

            orig_updateFader();
        }

        public void enableAutoPlay(bool isEnable)
        {
            isEnableAutoPlay = isEnable;
        }
    }
}
