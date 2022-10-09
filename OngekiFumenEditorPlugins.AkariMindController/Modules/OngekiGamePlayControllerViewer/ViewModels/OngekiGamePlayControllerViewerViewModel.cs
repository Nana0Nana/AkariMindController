using AkiraMindController.Communication;
using AkiraMindController.Communication.AkariCommand;
using AkiraMindController.Communication.Connectors.CommonMessages;
using AkiraMindController.Communication.Connectors.ConnectorImpls.Http;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Modules.FumenVisualEditor.Kernel;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Utils;
using OngekiFumenEditor.Utils.Attributes;
using OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer.Base;
using OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer.Views;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using static AkiraMindController.Communication.Connectors.CommonMessages.Ping;

namespace OngekiFumenEditorPlugins.AkariMindController.Modules.OngekiGamePlayControllerViewer.ViewModels
{
    [Export(typeof(IOngekiGamePlayControllerViewer))]
    [MapToView(ViewType = typeof(OngekiGamePlayControllerViewerView))]
    public class OngekiGamePlayControllerViewerViewModel : Tool, IOngekiGamePlayControllerViewer
    {
        public override PaneLocation PreferredLocation => PaneLocation.Bottom;

        public bool IsConnected => ConnectStatus == ConnectStatus.Connected;
        private ConnectStatus connectStatus = ConnectStatus.NotConnect;
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

        private string ogkrSavePath;
        public string OgkrSavePath
        {
            get => ogkrSavePath;
            set => Set(ref ogkrSavePath, value);
        }

        private FumenVisualEditorViewModel currentEditor;
        public FumenVisualEditorViewModel CurrentEditor
        {
            get => currentEditor;
            set => Set(ref currentEditor, value);
        }

        private bool isPlayGuideSEAfterPlay;
        public bool IsPlayGuideSEAfterPlay
        {
            get => isPlayGuideSEAfterPlay;
            set => Set(ref isPlayGuideSEAfterPlay, value);
        }

        private bool isAutoPlay;
        public bool IsAutoPlay
        {
            get => isAutoPlay;
            set
            {
                Set(ref isAutoPlay, value);
                MakeSureAutoPlayApplied();
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
        private HttpConnectorClient client;
        private AbortableThread thread;

        public int Port
        {
            get => port;
            set => Set(ref port, value);
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            DisplayName = "AkariMindController";
            IoC.Get<IEditorDocumentManager>().OnActivateEditorChanged += OngekiGamePlayControllerViewerViewModel_OnActivateEditorChanged;

            var opt = new JsonSerializerOptions() { IncludeFields = true };
            SimpleInterfaceImplement.Deserialize = (json, type) => JsonSerializer.Deserialize(json, type, opt);
            SimpleInterfaceImplement.Serialize = obj => JsonSerializer.Serialize(obj, opt);
            SimpleInterfaceImplement.Log = x => Log.LogDebug(x);

            thread = new AbortableThread(OnThreadAutomaticCheckConnect);
            thread.IsBackground = true;
            thread.Start();
        }

        private async void OnThreadAutomaticCheckConnect(CancellationToken obj)
        {
            while (!obj.IsCancellationRequested)
            {
                await Task.Delay(1000);
                if (client is null)
                    continue;
                await UpdateCheckConnecting();
            }
        }

        private void OngekiGamePlayControllerViewerViewModel_OnActivateEditorChanged(FumenVisualEditorViewModel @new, FumenVisualEditorViewModel old)
        {
            CurrentEditor = @new;
        }

        private void AppendOutputLine(string v)
        {
            Log.LogInfo($"[akari]{v}");
        }

        public async void Connect()
        {
            client = new HttpConnectorClient(Port);
            if (!await UpdateCheckConnecting())
            {
                client = default;
                ConnectStatus = ConnectStatus.Disconnected;
            }
            else
            {
                RefreshUI();
            }
        }

        public void OpenOgkrSavePathDialog()
        {
            OgkrSavePath = FileDialogHelper.SaveFile("请指定.ogkr保存的路径用于音击程序读取.", new[] { (".ogkr", "标准音击谱面文件") }) ?? OgkrSavePath;
        }

        public async void GetOgkrSavePathFromGamePlay()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;

            if ((await GetNotesManagerData())?.OgkrFilePath is not string ogkrPath || !File.Exists(ogkrPath))
                return;

            OgkrSavePath = Path.GetFullPath(ogkrPath);
            MessageBox.Show("获取成功");
        }

        private Task SendMessageAsync<T>() where T : new() => SendMessageAsync(new T());
        private Task SendMessageAsync(object obj) => Task.Run(() => client.SendMessage(obj));
        private Task<X> SendMessageAsync<T, X>() where T : new() where X : new() => SendMessageAsync<T, X>(new T());
        private Task<X> SendMessageAsync<T, X>(T obj) where T : new() where X : new() => Task.Run(() => client.SendMessageWithResponse<T, X>(obj));

        public async Task Play()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;

            await SendMessageAsync(new ResumeGamePlay() { playGuideSEBeforePlay = IsPlayGuideSEAfterPlay });
        }

        public async Task Pause()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;
            await SendMessageAsync(new PauseGamePlay());
        }

        public async Task Restart()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;

            await SendMessageAsync(new RestartGamePlay());
        }

        public async Task SeekTo(TimeSpan time)
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;

            Log.LogError($"akari seek to {time} , playAfterSeek : {IsPlayAfterSeek}");
            await SendMessageAsync(new SeekToGamePlay()
            {
                audioTimeMsec = (int)time.TotalMilliseconds,
                playAfterSeek = IsPlayAfterSeek
            });
        }

        public Task SeekTo() => SeekTo(TimeSpan.FromMilliseconds(SeekTimeMsec));

        public async void RefreshUI()
        {
            isAutoPlay = (await GetNotesManagerData())?.IsAutoPlay ?? false;
            NotifyOfPropertyChange(() => IsAutoPlay);
        }

        public async Task Reload()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;
            if (!File.Exists(OgkrSavePath))
                return;
            if ((await GetNotesManagerData()) is not NotesManagerData data)
                return;
            var currentPlaytime = data.CurrentTime;
            await GenerateOgkr(OgkrSavePath);
            await SeekTo(currentPlaytime);
        }

        private async Task MakeSureAutoPlayApplied()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;
            await SendMessageAsync(new AutoPlay() { isEnable = IsAutoPlay });
        }

        private async Task GenerateOgkr(string ogkrSavePath)
        {
            if (CurrentEditor is null)
                return;

            if (Type.GetType("OngekiFumenEditorPlugins.OngekiFumenSupport.Kernel.StandardizeFormat,OngekiFumenEditorPlugins.OngekiFumenSupport") is not Type type)
            {
                Log.LogError($"AkariMindController can't generate .ogkr because program not apply plugin named 'OngekiFumenSupport'.");
                return;
            }

            var method = type.GetMethod("Process").CreateDelegate<Func<OngekiFumen, Task<OngekiFumen>>>(null);
            var fumen = await method(CurrentEditor.Fumen);

            var data = await IoC.Get<IFumenParserManager>().GetSerializer(ogkrSavePath).SerializeAsync(fumen);
            await File.WriteAllBytesAsync(ogkrSavePath, data);
            await SendMessageAsync(new ReloadFumen { checkOgkrFilePath = ogkrSavePath });

            Log.LogError($"AkariMindController generate fumen to {ogkrSavePath}");
        }

        public async Task<bool> UpdateCheckConnecting()
        {
            ConnectStatus = (await SendMessageAsync<Ping, Pong>().WithTimeout(1000)) is Pong ?
                ConnectStatus.Connected : ConnectStatus.Disconnected;
            return ConnectStatus == ConnectStatus.Connected;
        }

        public Task<bool> CheckVailed()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return Task.FromResult(false);
            //todo
            return Task.FromResult(true);
        }

        public async void PlayGuideSE()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return;

            await SendMessageAsync<PlayGuideSE>();
        }

        public async Task<NotesManagerData?> GetNotesManagerData()
        {
            if (ConnectStatus != ConnectStatus.Connected)
                return null;

            if ((await SendMessageAsync<GetNoteManagerValue, GetNoteManagerValue.ReturnValue>()) is not GetNoteManagerValue.ReturnValue retVal)
                return null;

            var msecPerFrame = 1000 / 60.0;

            return new NotesManagerData()
            {
                CurrentTime = TimeSpan.FromMilliseconds(retVal.currentFrame * msecPerFrame),
                InvisibleTime = TimeSpan.FromMilliseconds(retVal.invisibleFrame * msecPerFrame),
                VisibleTime = TimeSpan.FromMilliseconds(retVal.visibleFrame * msecPerFrame),
                NoteEndTime = TimeSpan.FromMilliseconds(retVal.noteEndFrame * msecPerFrame),
                NoteStartTime = TimeSpan.FromMilliseconds(retVal.noteStartFrame * msecPerFrame),
                PlayEndTime = TimeSpan.FromMilliseconds(retVal.playEndFrame * msecPerFrame),
                PlayStartTime = TimeSpan.FromMilliseconds(retVal.playStartFrame * msecPerFrame),
                PlayProgress = retVal.playProgress,
                OgkrFilePath = retVal.ogkrFilePath,
                IsPlayEnd = retVal.isPlayEnd,
                IsPlaying = retVal.isPlaying,
                IsAutoPlay = retVal.autoPlay
            };
        }
    }
}
