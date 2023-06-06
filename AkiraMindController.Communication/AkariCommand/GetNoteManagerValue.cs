using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.AkariCommand
{
    [Serializable]
    public class GetNoteManagerValue
    {
        [Serializable]
        public class ReturnValue
        {
            public float playEndFrame;
            public float noteEndFrame;

            public float playStartFrame;
            public float noteStartFrame;

            public float visibleFrame;
            public float invisibleFrame;

            public float currentFrame;
            public float playProgress;

            public bool isPlaying;
            public bool isPlayEnd;

            public string ogkrFilePath;

            public float playerPosX;

            public float posInR;
            public float posInC;
            public float posInL;

            public bool autoPlay;
            public float autoFader;
            public bool isPauseIfMissBellOrDamaged;
        }
    }
}
