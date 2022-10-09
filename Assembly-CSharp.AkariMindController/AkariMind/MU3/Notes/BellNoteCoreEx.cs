using MonoMod;
using MU3.Notes;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.BellNoteCore")]
    internal class BellNoteCoreEx : BellNoteCore
    {
        public float getAvaliableFrameHit => isHadPallete ? bparam.frameHit : param.frame;
        public float getAvaliablePlaceHit => isHadPallete ? bparam.placeHit : param.place;
    }
}