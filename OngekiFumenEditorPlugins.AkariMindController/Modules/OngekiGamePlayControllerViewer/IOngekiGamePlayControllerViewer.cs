using Gemini.Framework;
using OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer
{
    public interface IOngekiGamePlayControllerViewer : ITool
    {
        Task Play();
        Task Pause();
        Task SeekTo(TimeSpan time);
        Task Reload();
        Task<bool> CheckVailedAndConnected();
        Task<TimeSpan> CheckVailed();
        Task<NotesManagerData> GetNotesManagerData();
    }
}
