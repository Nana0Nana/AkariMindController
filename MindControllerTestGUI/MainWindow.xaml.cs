using AkiraMindController.Communication;
using AkiraMindController.Communication.AkariCommand;
using AkiraMindController.Communication.Connectors.CommonMessages;
using AkiraMindController.Communication.Connectors.ConnectorImpls.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static AkiraMindController.Communication.Connectors.CommonMessages.Ping;

namespace MindControllerTestGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public bool IsConnected
        {
            get => isConnected;
            set
            {
                if (isConnected != value && !value)
                    AppendOutputLine("Disconnected.");
                isConnected = value;
                PropertyChanged?.Invoke(this, new(nameof(IsConnected)));
            }
        }

        public string Output
        {
            get { return (string)GetValue(OutputProperty); }
            set { SetValue(OutputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Output.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutputProperty =
            DependencyProperty.Register("Output", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Port.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(MainWindow), new PropertyMetadata(30000));

        public int SeekTime
        {
            get { return (int)GetValue(SeekTimeProperty); }
            set { SetValue(SeekTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeekTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeekTimeProperty =
            DependencyProperty.Register("SeekTime", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public bool IsPlayAfterSeek
        {
            get { return (bool)GetValue(IsPlayAfterSeekProperty); }
            set { SetValue(IsPlayAfterSeekProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPlayAfterSeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPlayAfterSeekProperty =
            DependencyProperty.Register("IsPlayAfterSeek", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        private HttpConnectorClient client;
        private bool isConnected;
        private DispatcherTimer timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            SimpleInterfaceImplement.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
            SimpleInterfaceImplement.Serialize = (obj) => JsonConvert.SerializeObject(obj);
            SimpleInterfaceImplement.Log = x => Debug.WriteLine(x);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += (a, b) =>
            {
                IsConnected = client?.SendMessageWithResponse<Ping, Pong>() is Pong;
            };

            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //pause
            client.SendMessage<PauseGamePlay>();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //connect 
            client = new HttpConnectorClient(Port);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //resume
            client.SendMessage<ResumeGamePlay>();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //restart
            client.SendMessage<RestartGamePlay>();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //getData
            var result = client.SendMessageWithResponse<GetNoteManagerValue, GetNoteManagerValue.ReturnValue>();
            AppendOutputLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //seek
            client.SendMessage(new SeekToGamePlay() { audioTimeMsec = SeekTime, playAfterSeek = IsPlayAfterSeek });
        }

        private async void AppendOutputLine(string content)
        {
            var isScrollToEnd = scrollViewer.VerticalOffset == scrollViewer.ContentVerticalOffset;
            Output += content;
            Output += Environment.NewLine;

            if (isScrollToEnd)
            {
                await Dispatcher.Yield();
                scrollViewer.ScrollToEnd();
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            //clear
            Output = "";
        }
    }
}
