using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.AkariCommand
{
    [Serializable]
    public class SeekToGamePlay
    {
        public int audioTimeMsec;
        public bool playAfterSeek;
    }
}
