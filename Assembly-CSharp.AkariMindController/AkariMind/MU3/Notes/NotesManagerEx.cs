using AkariMindControllers.Utils;
using AkiraMindController.Communication.Bases;
using AkiraMindController.Communication.Utils;
using JetBrains.Annotations;
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
using UnityEngine;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.NotesManager")]
    internal partial class NotesManagerEx : NotesManager
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

        public extern void orig_reset();
        public void reset()
        {
            orig_reset();
            curFaderTarget = default;
            prevFaderTarget = default;
        }

        private extern void orig_createFieldState(float frame, ref FieldState fieldState);
        private void createFieldState(float frame, ref FieldState fieldState) => orig_createFieldState(frame, ref fieldState);

        public extern void orig_damagePlayer(Damage type, int damageXM);

        void print(AutoFaderTarget r)
        {
            PatchLog.WriteLine($"finalTargetPlace : {r.targetPlaceRange}");
            PatchLog.WriteLine($"finalTargetFrame : {r.finalTargetFrame}");

            PatchLog.WriteLine($"moveableRange : {r.moveableRange}");
            PatchLog.WriteLine($"damageRanges : {string.Join(" ", r.damageRanges.Select(x => x.ToString()).ToArray())}");
            PatchLog.WriteLine($"bellRanges : {string.Join(" ", r.bellRanges.Select(x => x.ToString()).ToArray())}");
            PatchLog.WriteLine($"targetRanges : {string.Join(" ", r.targetRanges.Select(x => x.ToString()).ToArray())}");
        }

        public void damagePlayer(Damage type, int damageXM)
        {
            orig_damagePlayer(type, damageXM);

            if (isAutoPlay())
            {
                PatchLog.WriteLine($"------------damagePlayer() dumper-----------");
                PatchLog.WriteLine($"call damagePlayer({type},{damageXM}) at _curFrame:{getCurrentFrame()} ({getCurrentMsec()}ms)");
                PatchLog.WriteLine($"------------curFaderTarget-----------");
                print(curFaderTarget);
                PatchLog.WriteLine($"------------prevFaderTarget-----------");
                print(prevFaderTarget);
                PatchLog.WriteLine($"--------------------------------------------");
            }
        }

        public extern void orig_setResultEffectAndScore_Bell(bool isHit, int recoverXM, Vector3 posText, Vector3 posBomb);
        public virtual void setResultEffectAndScore_Bell(bool isHit, int recoverXM, Vector3 posText, Vector3 posBomb)
        {
            orig_setResultEffectAndScore_Bell(isHit, recoverXM, posText, posBomb);
            if (!isHit)
            {
                if (isAutoPlay())
                {
                    PatchLog.WriteLine($"------------setResultEffectAndScore_Bell() dumper-----------");
                    PatchLog.WriteLine($"call setResultEffectAndScore_Bell({isHit},{recoverXM},{posText},{posBomb}) at _curFrame:{getCurrentFrame()} ({getCurrentMsec()}ms)");
                    PatchLog.WriteLine($"------------curFaderTarget-----------");
                    print(curFaderTarget);
                    PatchLog.WriteLine($"------------prevFaderTarget-----------");
                    print(prevFaderTarget);
                    PatchLog.WriteLine($"--------------------------------------------");
                }
            }
        }

        public float fakeButtomMsec = 500;
        public float fakeButtomOffsetLen = 2;

        public struct NoteFrameInfo
        {
            public float frame;
            public NotesBase note;
        }

        public AutoFaderTarget curFaderTarget = default;
        public AutoFaderTarget prevFaderTarget = default;

        private float calcAutoPlayFader()
        {
            var curFrame = _curFrame;
            var currentFaderPlace = SingletonMonoBehaviour<GameEngine>.instance.gameDeviceManager.getFader();

            if (curFrame > curFaderTarget.finalTargetFrame)
            {
                /*
                    获取未来500ms内，最早出现的Bell或者伤害物件,
                    如果没有的话就随便
                    因为激光是持续性的，因此如果在激光持续时间内则一直计算
                 */
                var nextFrame = curFrame + 500 / 16.666667f;
                var filterNote = _pNotesList.Select(x => x switch
                {
                    ShellNoteEx shell => new NoteFrameInfo { frame = shell.ShellNoteCore.param.frameHit, note = shell },
                    BellNoteEx bell => new NoteFrameInfo { frame = bell.BellNoteCore.getAvaliableFrameHit, note = bell },
                    BeamNoteEx beam => new NoteFrameInfo
                    {
                        frame = beam.ShellNoteCore.param.placeFore <= curFrame &&
                                        curFrame <= beam.ShellNoteCore.param.placeRear ?
                                            curFrame :
                                            beam.ShellNoteCore.param.placeFore,
                        note = beam
                    },
                    _ => default
                }).Where(x => x.note is not null && curFrame < x.frame && x.frame <= nextFrame).ToArray();

                var minNextFrame = filterNote.Length == 0 ? nextFrame : filterNote.Min(x => x.frame);
                nextFrame = Math.Min(getEndPlayFrame(), minNextFrame);

                var getDamageNotes = filterNote.Where(x => x.frame == nextFrame)
                              .Select(x => x.note)
                              .OfType<ShellNoteEx>()
                              .ToArray();

                var getBeamNotes = filterNote/*.Where(x => x.frame == minNextFrame)*/
                              .Select(x => x.note)
                              .OfType<BeamNoteEx>()
                              .ToArray();

                var getBellNotes = filterNote.Where(x => x.frame == nextFrame)
                              .Select(x => x.note)
                              .OfType<BellNoteEx>()
                              .ToArray();

                var shellFieldState = new FieldState()
                {
                    area = new FieldObject.Area()
                };

                createFieldState(nextFrame, ref shellFieldState);

                var damageRanges = Enumerable.Empty<ValueRange>();

                if (getDamageNotes.Length > 0)
                {
                    //子弹
                    damageRanges = getDamageNotes.Select(x =>
                    {
                        var width = x.ShellNoteCore.ShellWidth;
                        var place = x.ShellNoteCore.param.placeHit;
                        return new ValueRange(place - width * 0.5f, place + width * 0.5f);
                    });
                }

                //激光
                damageRanges = damageRanges.Concat(getBeamNotes.Select(x =>
                {
                    var place = x.ShellNoteCore.param.shape.getPlace(getCurrentMsec());
                    var width = x.ShellNoteCore.param.widthJudge;
                    return new ValueRange(place - width * 0.5f, place + width * 0.5f);
                }));

                //伤害区域
                damageRanges = ValueRange.Union(damageRanges).ToArray();

                //Bell(必碰)区域
                var bellRanges = Enumerable.Empty<ValueRange>();
                if (getBellNotes.Length > 0)
                {
                    bellRanges = ValueRange.Union(getBellNotes.Select(x =>
                    {
                        var width = 5;
                        var place = x.BellNoteCore.getAvaliablePlaceHit;
                        return new ValueRange(place - width * 0.5f, place + width * 0.5f);
                    }));
                }

                //摇杆可移动区域
                var movableRange = new ValueRange(shellFieldState.area.posCenterL, shellFieldState.area.posCenterR);

                //安全区域 = 可移动区域 - 伤害区域
                var safeRanges = ValueRange.Except(movableRange, damageRanges);

                //必去区域 = 安全区域 和 Bell区域(如果有的话) 的交集
                var targetRanges = safeRanges;
                if (bellRanges.Any())
                    targetRanges = ValueRange.Intersect(safeRanges.Concat(bellRanges));

                //选择必去区域最近的一个点去插值
                var calcRanges = targetRanges.Select(x =>
                {
                    //x表示排序依据，表示中点位置
                    return new Vector3(x.max - x.min, x.min, x.max);
                }).OrderByDescending(x => x.x).ToArray();

                var targetPlaceRange = calcRanges.Select(x => new ValueRange(x.y, x.z)).FirstOrDefault();

                var newFaderTarget = new AutoFaderTarget();
                newFaderTarget.finalTargetFrame = nextFrame;
                newFaderTarget.targetPlaceRange = targetPlaceRange;

                //record result
                newFaderTarget.bellRanges = bellRanges.ToArray();
                newFaderTarget.targetRanges = targetRanges.ToArray();
                newFaderTarget.damageRanges = damageRanges.ToArray();

                newFaderTarget.moveableRange = movableRange;

                prevFaderTarget = curFaderTarget;
                curFaderTarget = newFaderTarget;

                if (prevFaderTarget.finalTargetFrame == curFaderTarget.finalTargetFrame)
                {
                    //warn!!!
                    curFaderTarget = newFaderTarget;
                }
            }

            //calc actualX
            double adjustPlace = Math.Max(fieldState.area.posCenterL, Math.Min(fieldState.area.posCenterR, currentFaderPlace));
            if (!(curFaderTarget.targetPlaceRange.min * 0.5 <= adjustPlace && adjustPlace <= curFaderTarget.targetPlaceRange.max * 0.5))
            {
                var prevPlaceTarget = (prevFaderTarget.targetPlaceRange.min + prevFaderTarget.targetPlaceRange.max) / 2;
                var curPlaceTarget = (curFaderTarget.targetPlaceRange.min + curFaderTarget.targetPlaceRange.max) / 2;

                adjustPlace = MathUtils.CalculateXFromTwoPointFormFormula(curFrame, prevPlaceTarget, prevFaderTarget.finalTargetFrame, curPlaceTarget, curFaderTarget.finalTargetFrame);
            }

            adjustPlace = Math.Max(fieldState.area.posCenterL, Math.Min(fieldState.area.posCenterR, adjustPlace));

            return (float)adjustPlace;
        }

        public float autoFaderPre = 0;

        private void updateFader()
        {
            var gameObj = SingletonMonoBehaviour<GameEngine>.instance.gameDeviceManager as GameDeviceManagerEx;
            var autoFader = autoFaderPre = calcAutoPlayFader();

            if (isAutoPlay())
                gameObj?.setFader(autoFader);

            orig_updateFader();
        }

        public void enableAutoPlay(bool isEnable)
        {
            isEnableAutoPlay = isEnable;
        }
    }
}
