using AkiraMindController.Communication.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.AkariCommand
{
    [Serializable]
    public class GetNoteManagerAutoPlayData
    {
        [Serializable]
        public class ReturnValue
        {
            public bool autoPlay;
            public float autoFader;

            public string curFaderTargetStr;
            public string prevFaderTargetStr;
        }
    }
}
