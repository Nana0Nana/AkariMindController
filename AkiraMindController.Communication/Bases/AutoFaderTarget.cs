using AkiraMindController.Communication.Utils;
using System;

namespace AkiraMindController.Communication.Bases
{
    [Serializable]
    public struct AutoFaderTarget
    {
        //just for debugger inspector
        public ValueRange[] damageRanges;
        public ValueRange[] bellRanges;
        public ValueRange[] targetRanges;

        public ValueRange moveableRange;

        public float finalTargetPlace;
        public float finalTargetFrame;
    }
}

