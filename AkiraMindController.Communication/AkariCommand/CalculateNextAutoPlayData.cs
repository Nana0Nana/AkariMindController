using AkiraMindController.Communication.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkiraMindController.Communication.AkariCommand
{
    [Serializable]
    public class CalculateNextAutoPlayData
    {
        public float frame;

        [Serializable]
        public class ReturnValue
        {
            public string curFaderTargetStr;
        }
    }
}
