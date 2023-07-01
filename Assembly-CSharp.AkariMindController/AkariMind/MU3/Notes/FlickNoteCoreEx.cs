using MonoMod;
using MU3.Notes;

namespace AkariMindControllers.AkariMind.MU3.Notes
{
    [MonoModPatch("global::MU3.Notes.FlickNoteCore")]
    public class FlickNoteCoreEx : FlickNoteCore
    {
    }
}