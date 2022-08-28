using Gemini.Framework;
using Gemini.Framework.Services;
using OngekiFumenEditor.Utils.Attributes;
using OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer.Base;
using OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer.Views;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer.ViewModels
{
    [Export(typeof(IOngekiGamePlayControllerViewer))]
    [MapToView(ViewType = typeof(OngekiGamePlayControllerViewerView))]
    public class OngekiGamePlayControllerViewerViewModel : Tool, IOngekiGamePlayControllerViewer
    {
        public override PaneLocation PreferredLocation => PaneLocation.Bottom;

        public bool IsConnected => ConnectStatus == ConnectStatus.Connected;
        private ConnectStatus connectStatus;
        public ConnectStatus ConnectStatus
        {
            get => connectStatus;
            set
            {
                if (connectStatus != value && value == ConnectStatus.Disconnected)
                    AppendOutputLine("Disconnected.");
                Set(ref connectStatus, value);
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        private int seekTimeMsec;
        public int SeekTimeMsec
        {
            get => seekTimeMsec;
            set => Set(ref seekTimeMsec, value);
        }

        private bool isPlayAfterSeek;
        public bool IsPlayAfterSeek
        {
            get => isPlayAfterSeek;
            set => Set(ref isPlayAfterSeek, value);
        }

        private int port = 30000;
        public int Port
        {
            get => port;
            set => Set(ref port, value);
        }

        private void AppendOutputLine(string v)
        {
            OngekiFumenEditor.Utils.Log.LogInfo($"[akari]{v}");
        }

        public OngekiGamePlayControllerViewerViewModel()
        {
            DisplayName = "AkariMindController";
        }

        public void Connect()
        {

        }

        public Task Play()
        {
            throw new NotImplementedException();
        }

        public Task Pause()
        {
            throw new NotImplementedException();
        }

        public Task Restart()
        {
            throw new NotImplementedException();
        }

        public Task SeekTo(TimeSpan time)
        {
            throw new NotImplementedException();
        }

        public Task SeekTo() => SeekTo(TimeSpan.FromMilliseconds(SeekTimeMsec));

        public Task Reload()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckVailedAndConnected()
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> CheckVailed()
        {
            throw new NotImplementedException();
        }

        public Task<NotesManagerData> GetNotesManagerData()
        {
            throw new NotImplementedException();
        }
    }
}
