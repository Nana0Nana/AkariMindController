using AkariMindControllers.AkariMind.MU3.Notes;
using AkiraMindController.Communication.AkariCommand;
using AkiraMindController.Communication.Connectors;
using MonoMod;
using MU3.Battle;
using MU3.Data;
using MU3.Game;
using MU3.Notes;
using MU3.Reader;
using MU3.Sequence;
using MU3.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AkariMindControllers.AkariMind.MU3.Sequence
{
    [MonoModPatch("global::MU3.Sequence.PlayMusic")]
    internal class PlayMusicPatch : PlayMusic
    {
        private GameEngine _gameEngine;
        private SessionInfo _sessionInfo;

        private NotesManager ntMgr => (!(_gameEngine != null)) ? null : _gameEngine.notesManager;
        private NotesManagerEx ntMgrEx => ntMgr as NotesManagerEx;

        private bool isPause = false;
        private float pauseMsec;

        private extern void orig_Enter_Play();

        private void Enter_Play()
        {
            orig_Enter_Play();

            //register messages.
            Controller.RegisterMessageHandler<RestartGamePlay>(OnRequestRestartGamePlay);
            Controller.RegisterMessageHandler<ResumeGamePlay>(OnRequestResumeGamePlay);
            Controller.RegisterMessageHandler<PauseGamePlay>(OnRequestPauseGamePlay);
            Controller.RegisterMessageHandler<PrintGamePlayStatus>(OnRequestPrintGamePlayStatus);
            Controller.RegisterMessageHandler<ReloadFumen>(OnReloadFumen);
            Controller.RegisterMessageHandler<GetNoteManagerValue>(OnRequestGetNoteManagerValue);
            Controller.RegisterMessageHandler<SeekToGamePlay>(OnRequestSeekToGamePlay);

            isPause = false;
        }

        private void OnReloadFumen(ReloadFumen message, IResponser responser)
        {
            PatchLog.WriteLine($"call OnReloadFumen() message.checkOgkrFilePath = {message.checkOgkrFilePath}");
            Singleton<ReaderMain>.instance.loadScore(message.checkOgkrFilePath);
        }

        private void OnRequestGetNoteManagerValue(GetNoteManagerValue message, IResponser responser)
        {
            responser.Response(new GetNoteManagerValue.ReturnValue()
            {
                playEndFrame = ntMgr.getEndPlayFrame(),
                noteEndFrame = ntMgr.getEndNoteFrame(),
                playStartFrame = ntMgrEx.getStartPlayFrame(),
                noteStartFrame = ntMgrEx.getStartNoteFrame(),
                visibleFrame = ntMgrEx.getFrameVisible(),
                invisibleFrame = ntMgrEx.getFrameInvisible(),
                currentFrame = ntMgrEx.getCurrentFrame(),
                playProgress = ntMgrEx.getPlayProgress(),
                isPlaying = ntMgrEx.isPlaying,
                isPlayEnd = ntMgrEx.isPlayEnd,
                ogkrFilePath = SingletonStateMachine<DataManager, DataManager.EState>.instance.getOgkrPath(_sessionInfo.musicData.id, _sessionInfo.musicLevel)
            });
        }

        private void OnRequestPrintGamePlayStatus(PrintGamePlayStatus message)
        {
            PatchLog.WriteLine($"call OnRequestPrintGamePlayStatus()");
            PatchLog.WriteLine($"isPause : {isPause}");
            PatchLog.WriteLine($"pauseMsec : {pauseMsec}");
            PatchLog.WriteLine($"Singleton<ReaderMain>.instance.enable : {Singleton<ReaderMain>.instance.enable}");
            PatchLog.WriteLine($"ntMgr.getCurrentFrame() : {ntMgr.getCurrentFrame()}");
            PatchLog.WriteLine($"ntMgr.isPlaying : {ntMgr.isPlaying}");
            PatchLog.WriteLine($"ntMgr.isPlayEnd : {ntMgr.isPlayEnd}");
            PatchLog.WriteLine($"ntMgr.getAddFrame() : {ntMgr.getAddFrame()}");
            PatchLog.WriteLine($"ntMgr.getCurrentMsec() : {ntMgr.getCurrentMsec()}");
            var gameSound = Singleton<GameSound>.instance;
            PatchLog.WriteLine($"gameBGM.isPlay : {gameSound.gameBGM.isPlay}");
            PatchLog.WriteLine($"gameBGM.msec : {gameSound.gameBGM.msec}");
        }

        private void PauseGameInternal()
        {
            if (isPause)
                return;
            var bgm = Singleton<GameSound>.instance.gameBGM;
            pauseMsec = bgm.msec;
            bgm.stop();
            ntMgr.setPause(true);

            isPause = true;
        }

        private void ResumeGameInternal()
        {
            if (!isPause)
                return;

            Singleton<GameSound>.instance.gameBGM.playMusic(_sessionInfo.musicData, (int)pauseMsec);
            ntMgr.setPause(false);
            ntMgr.startPlay(pauseMsec);

            isPause = false;
        }

        private IEnumerator OnRequestSeekToGamePlay(SeekToGamePlay message)
        {
            PauseGameInternal();
            var msec = message.audioTimeMsec;

            yield return null;
            //clean objects and reset status
            ntMgr.reset();
            //reload&parse fumen file again
            ntMgr.loadScore(_sessionInfo, _gameEngine.IsStageDazzling);
            yield return null;
            //reset counter
            _gameEngine.reset();
            //seek timeline of notes
            ntMgr.setFrameForce(msec / 16.666666f);

            //redraw notes and make them visible
            ntMgrEx.refreshNotesVisible();

            pauseMsec = msec;

            if (message.playAfterSeek)
                ResumeGameInternal();

            isPause = !message.playAfterSeek;
        }

        private void OnRequestPauseGamePlay(PauseGamePlay message)
        {
            PatchLog.WriteLine($"call OnRequestPauseGamePlay() isPause = {isPause}");
            PauseGameInternal();
            PatchLog.WriteLine($"pause game , pauseMsec = {pauseMsec:F4} , ntMgr.currentMsec = {ntMgr.getCurrentMsec():F4} , ntMgr.currentFrame = {ntMgr.getCurrentFrame():F4}");
        }

        private void OnRequestResumeGamePlay(ResumeGamePlay message)
        {
            PatchLog.WriteLine($"call OnRequestResumeGamePlay() isPause = {isPause} pauseMsec:{pauseMsec}");
            ResumeGameInternal();
        }

        private void Leave_Play()
        {
            //unregister messages.
            Controller.UnregisterSpecifyMessageAllHandler<RestartGamePlay>();
            Controller.UnregisterSpecifyMessageAllHandler<ResumeGamePlay>();
            Controller.UnregisterSpecifyMessageAllHandler<PauseGamePlay>();
            Controller.UnregisterSpecifyMessageAllHandler<SeekToGamePlay>();
            Controller.UnregisterSpecifyMessageAllHandler<PrintGamePlayStatus>();
            Controller.UnregisterSpecifyMessageAllHandler<GetNoteManagerValue>();
            Controller.UnregisterSpecifyMessageAllHandler<ReloadFumen>();
        }

        private IEnumerator OnRequestRestartGamePlay(RestartGamePlay message)
        {
            PatchLog.WriteLine("call OnRequestRestartGamePlay()");
            yield return null;
            Singleton<GameSound>.instance.gameBGM.stop();
            ntMgr.stopPlay();
            yield return null;
            _gameEngine.finishGame();
            _gameEngine.playFinish();
            base.setNextState(EState.Init);
        }
    }
}
