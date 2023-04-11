using CommunityToolkit.Mvvm.Messaging;
using CTrue.FsConnect;
using LsideWPF.Common;
using LsideWPF.Models;
using LsideWPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using static LsideWPF.Models.Events;

namespace LsideWPF.Services
{
    public class SimService : BindableBase, ISimService, INotifyPropertyChanged
    {

        private ILandingLoggerService landingLoggerService = App.Current.Services.GetService<ILandingLoggerService>();

        // timer, task reads data from a SimConnection
        readonly DispatcherTimer dataReadDispatchTimer = new DispatcherTimer();

        // timer, task establishes connection if disconnected
        readonly DispatcherTimer ConnectionDispatchTimer = new DispatcherTimer();

        // Establishes simConnect connection & Update scaneris
        readonly BackgroundWorker backgroundWorkerConnection = new BackgroundWorker();

        // flag to ensure only one SimConnect data packet being processed at a time
        static bool SafeToRead = true;

        static StateMachine _stateMachine;
        static event EventHandler<FlightEventArgs> EventHandler;

        static readonly FsConnect fsConnect = new FsConnect();
        static readonly List<SimVar> definition = new List<SimVar>();
        static int planeInfoDefinitionId;

        const int SAMPLE_RATE = 20; //ms

        bool running = false;

        bool _connected = false;

        private enum Requests { PlaneInfoRequest = 0 }

        public SimService()
        {
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
            // Wind component in aircraft lateral (X) axis.
            definition.Add(new SimVar(FsSimVar.AircraftWindX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            // Wind component in aircraft longitudinal(Z) axis.
            definition.Add(new SimVar(FsSimVar.AircraftWindZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.AirspeedIndicated, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            // Speed relative to the earths surface.
            definition.Add(new SimVar(FsSimVar.GroundVelocity, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            // lateral speed + to the right
            definition.Add(new SimVar(FsSimVar.VelocityBodyX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            // speed along airplane axis
            definition.Add(new SimVar(FsSimVar.VelocityBodyZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.GForce, FsUnit.GForce, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneTouchdownNormalVelocity, FsUnit.FeetPerSecond, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneAltitudeAboveGround, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneLatitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.PlaneLongitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));

            definition.Add(new SimVar(FsSimVar.PlaneBankDegrees, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.OnAnyRunway, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32));
            definition.Add(new SimVar(FsSimVar.AtcRunwayAirportName, null, SIMCONNECT_DATATYPE.STRING256));
            //definition.Add(new SimVar(FsSimVar.AtcRunwayRelativePositionX, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            //definition.Add(new SimVar(FsSimVar.AtcRunwayRelativePositionZ, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.AtcRunwaySelected, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32));
            definition.Add(new SimVar(FsSimVar.AtcRunwayTdpointRelativePositionX, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            //definition.Add(new SimVar(FsSimVar.AtcRunwayTdpointRelativePositionY, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.AtcRunwayTdpointRelativePositionZ, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.RelativeWindVelocityBodyX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.RelativeWindVelocityBodyZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.AmbientWindX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.AmbientWindZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.RelativeWindVelocityBodyY, FsUnit.FeetPerMinute, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimVar(FsSimVar.GearPosition, FsUnit.Enum, SIMCONNECT_DATATYPE.INT32));

        }

        public bool Connected
        {
            get { return _connected; }
            private set { _connected = value; }
        }

        private static void HandleReceivedFsData(object sender, FsDataReceivedEventArgs e)
        {
            if (!SafeToRead)
            {
                // already processing a packet, skip this one
                Log.Debug("lost one");
                return;
            }
            SafeToRead = false;
            try
            {
                if (e.RequestId == (uint)Requests.PlaneInfoRequest)
                {
                    _stateMachine.Handle((PlaneInfoResponse)e.Data.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message);
            }
            SafeToRead = true;
        }

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

        private void ConnectionCheckEventHandler_OnTick(object sender, EventArgs e)
        {
            if (!backgroundWorkerConnection.IsBusy)
                backgroundWorkerConnection.RunWorkerAsync();

            bool oldConnected = Connected;

            if (fsConnect.Connected)
            {
                if (!running)
                {
                    EventHandler += FlightEventHandler;

                    _stateMachine = new StateMachine(new ConnectedState(), EventHandler);
                    running = true;
                    dataReadDispatchTimer.Start();
                }
                Connected = true;
            }
            else
            {
                Connected = false;
            }

            if (Connected != oldConnected)
                OnPropertyChanged(nameof(Connected));
        }

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
                    if (Properties.Settings.Default.enableTouchAndGo)
                    {
                        var flightParameters = StateMachine.ToFlightParameters(e.stateMachine);
                        WeakReferenceMessenger.Default.Send(new TouchAndGoEventMessage(flightParameters));

                        // viewModel.SetParameters(e.stateMachine);
                        // winLRM.SlideLeft();
                    }
                    break;

                case EventType.LandingEvent:
                    Log.Debug("Landing Event");
                    // update & reveil viewModels
                    {
                        var flightParameters = StateMachine.ToFlightParameters(e.stateMachine);

                        WeakReferenceMessenger.Default.Send(new LandingEventMessage(flightParameters));

                    }
                    break;

                case EventType.SlipLoggingEvent:
                    Log.Debug("Slip Logging Event");

                    // slipLogger.Add(e.planeInfoResponse);
                    // TODO

                    break;

                default:
                    break;
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
    }
}
