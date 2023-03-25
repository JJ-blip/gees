using CTrue.FsConnect;
using LsideWPF.model;
using Microsoft.FlightSimulator.SimConnect;
using Octokit;
//using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using static LsideWPF.model.Events;

using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;

namespace LsideWPF
{
    public enum Requests
    {
        PlaneInfoRequest = 0
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneInfoResponse
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Type;
        public bool OnGround;
        public double CrossWind;
        public double HeadWind;
        public double AirspeedInd;
        public double GroundSpeed;
        public double LateralSpeed;
        public double SpeedAlongHeading;
        public double Gforce;
        public double LandingRate;
        public double AltitudeAboveGround;
        public double Latitude;
        public double Longitude;

        public override string ToString()
        {
            return $"response OnGround:{OnGround}, AltitudeAboveGround:{AltitudeAboveGround}, AirspeedInd: {AirspeedInd} LandingRate: {LandingRate}";
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Publics and statics
        // flag to ensure only one SimConnect data packet being processed at a time
        static bool SafeToRead = true;

        static readonly FsConnect fsConnect = new FsConnect();
        static readonly List<SimVar> definition = new List<SimVar>();
        static int planeInfoDefinitionId;

        static StateMachine stateMachine;
        static event EventHandler<FlightEventArgs> EventHandler;

        static string updateUri;

        bool running = false;

        const int SAMPLE_RATE = 20; //ms

        // timer, task reads data from a SimConnection
        readonly DispatcherTimer dataReadDispatchTimer = new DispatcherTimer();

        // timer, task establishes connection if disconnected
        readonly DispatcherTimer ConnectionDispatchTimer = new DispatcherTimer();

        // Establishes simConnect connection & Update scaneris
        readonly BackgroundWorker backgroundWorkerConnection = new BackgroundWorker();

        // Background Git updated
        readonly BackgroundWorker backgroundWorkerUpdate = new BackgroundWorker();
        readonly NotifyIcon notifyIcon = new NotifyIcon();
        #endregion

        public ViewModel viewModel;
        public LandingViewModel landingViewModel;

        readonly LRMDisplay winLRM;

        private bool mouseDown;

        public MainWindow()
        {
            bool createdNew = true;
            var mutex = new Mutex(true, "Lside", out createdNew);
            if (!createdNew)
            {
                System.Windows.MessageBox.Show("App is already running.", "Lside", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }
            Log.Logger = new LoggerConfiguration()
                      .MinimumLevel.Debug()
                      .WriteTo.Console()
                     .CreateLogger();

            viewModel = new ViewModel();
            landingViewModel = new LandingViewModel();

            this.DataContext = viewModel;
            InitializeComponent();

            //POSITION
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;

            //Git Hub Updater, runs once immediately
            backgroundWorkerUpdate.DoWork += BackgroundWorkerUpdate_DoWork;
            backgroundWorkerUpdate.RunWorkerAsync();

            //do a 'Connection check' every 1 sec 
            ConnectionDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            ConnectionDispatchTimer.Tick += new EventHandler(ConnectionCheckEventHandler_OnTick);

            // establishes simConnect connection, when required
            backgroundWorkerConnection.DoWork += BackgroundWorkerConnection_DoWork;
            ConnectionDispatchTimer.Start();

            //Read SimConnect Data every 20 msec
            dataReadDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, SAMPLE_RATE);
            dataReadDispatchTimer.Tick += new EventHandler(DataReadEventHandler_OnTick);

            // register the read SimConnect data callback procedure
            fsConnect.FsDataReceived += HandleReceivedFsData;

            // properties to be read from SimConnect
            definition.Add(new SimVar(FsSimVar.Title, null, SIMCONNECT_DATATYPE.STRING256));
            definition.Add(new SimVar(FsSimVar.SimOnGround, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32));
            definition.Add(new SimVar(FsSimVar.AircraftWindX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.AircraftWindZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.AirspeedIndicated, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.GroundVelocity, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.VelocityBodyX, FsUnit.FeetPerSecond, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.VelocityBodyZ, FsUnit.FeetPerSecond, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.GForce, FsUnit.GForce, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneTouchdownNormalVelocity, FsUnit.FeetPerSecond, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneAltitudeAboveGround, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneLatitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneLongitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));

            //SHOW Slideable Landing Rate Monitor (LRM)
            winLRM = new LRMDisplay(viewModel);
            winLRM.Show();
        }

        #region MainWindow drag & drop

        private void Header_LoadedHandler(object sender, RoutedEventArgs e)
        {
            InitHeader(sender as TextBlock);
        }
        private void InitHeader(TextBlock header)
        {
            header.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            header.MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDown);
            header.MouseMove += new MouseEventHandler(OnMouseMove);
        }
        private new void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                mouseDown = true;
            }

            DragMove();
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                var mouseX = e.GetPosition(this).X;
                var width = RestoreBounds.Width;
                var x = mouseX - width / 2;

                if (x < 0)
                {
                    x = 0;
                }
                else
                if (x + width > SystemParameters.PrimaryScreenWidth)
                {
                    x = SystemParameters.PrimaryScreenWidth - width;
                }
                WindowState = WindowState.Normal;
                Left = x;
                Top = 0;
                DragMove();
            }
        }

        #endregion

        #region Reading and processing simconnect data
        private void DataReadEventHandler_OnTick(object sender, EventArgs e)
        {
            try
            {
                fsConnect.RequestData((int)Requests.PlaneInfoRequest, planeInfoDefinitionId);
            }
            catch
            {
            }
        }

        // Call back procedure to new handle SimConnect data
        private static void HandleReceivedFsData(object sender, FsDataReceivedEventArgs e)
        {
            if (!SafeToRead)
            {
                // already processing a packet, skip this one
                Console.WriteLine("lost one");
                return;
            }
            SafeToRead = false;
            try
            {
                if (e.RequestId == (uint)Requests.PlaneInfoRequest)
                {
                    stateMachine.Handle((PlaneInfoResponse)e.Data.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            SafeToRead = true;
        }

        #endregion

        #region Sim Connection
        private void ConnectionCheckEventHandler_OnTick(object sender, EventArgs e)
        {
            if (!backgroundWorkerConnection.IsBusy)
                backgroundWorkerConnection.RunWorkerAsync();

            if (fsConnect.Connected)
            {
                if (!running)
                {
                    EventHandler += FlightEventHandler;

                    stateMachine = new StateMachine(new ConnectedState(), EventHandler);
                    running = true;
                    dataReadDispatchTimer.Start();
                }

                notifyIcon.Icon = Properties.Resources.online;
                viewModel.Connected = true;
            }
            else
            {
                notifyIcon.Icon = Properties.Resources.offline;
                viewModel.Connected = false;
            }
        }

        // If not connected , Connect
        private void BackgroundWorkerConnection_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!fsConnect.Connected)
            {
                try
                {
                    // connect & register data of interest
                    fsConnect.Connect("TestApp", "localhost", 500, SimConnectProtocol.Ipv4);
                    planeInfoDefinitionId = fsConnect.RegisterDataDefinition<PlaneInfoResponse>(Requests.PlaneInfoRequest, definition);
                }
                catch { } // ignore
            }
        }
        #endregion

        #region Handlers for UI
        private void Button_Hide_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void RedditLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.reddit.com/r/MSFS2020LandingRate/");
        }
        private void GithubLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/scelts/lside");
        }
        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(updateUri);
        }
        private void ButtonLandings_Click(object sender, RoutedEventArgs e)
        {
            LandingsWindow landingWindow = new LandingsWindow(landingViewModel);
            landingWindow.Show();
        }
        private void ButtonShowLast_Click(object sender, RoutedEventArgs e)
        {
            // refresh model & displays it
            viewModel.SetParametersFromCVS();
            winLRM.SlideLeft();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(textBox.Text, out _))
            {
                Properties.Settings.Default.Save();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out _))
            {
            }
            else
            {
                e.Handled = true;
            }
        }
        private void ComboBoxScreens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Event Handlers

        protected virtual void FlightEventHandler(object sender, FlightEventArgs e)
        {
            if (e == null) return;

            switch (e.eventType)
            {
                case EventType.TakeOffEvent:
                    Log.Debug("Take Off Event");
                    // do nothing
                    break;

                case EventType.TouchAndGoEvent:
                    Log.Debug("Touch And Go Event");
                    // update & reveil viewModel
                    viewModel.SetParameters(e.stateMachine);
                    winLRM.SlideLeft();
                    break;

                case EventType.LandingEvent:
                    Log.Debug("Landing Event");
                    // update & reveil viewModels
                    landingViewModel.LogParams(e.stateMachine);
                    viewModel.SetParameters(e.stateMachine);
                    winLRM.SlideLeft();
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Git Hub Updater, amends displayed URL in the Main Window.
        private void BackgroundWorkerUpdate_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var client = new GitHubClient(new ProductHeaderValue("Lside"));
            var releases = client.Repository.Release.GetAll("jj-blip", "lside").Result;
            var latest = releases[0];
            viewModel.Updatable = viewModel.Version != latest.TagName;
            updateUri = latest.HtmlUrl;
        }

        #endregion

    }
}
