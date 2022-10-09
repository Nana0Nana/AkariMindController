using AkariMindControllers.Utils;
using AkiraMindController.Communication.Utils;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    internal partial class NotesManagerEx
    {
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
}
