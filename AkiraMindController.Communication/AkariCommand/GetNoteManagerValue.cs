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
            public float currentMusicId;
            public float playEndFrame;
            public float noteEndFrame;
            public float playStartFrame;
            public float noteStartFrame;
        }
    }
}
