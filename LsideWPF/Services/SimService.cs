namespace LsideWPF.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Threading;
    using CommunityToolkit.Mvvm.Messaging;
    using CTrue.FsConnect;
    using Microsoft.FlightSimulator.SimConnect;
    using Serilog;
    using static LsideWPF.Services.Events;

    /// <summary>
    /// Publishes following events:
    ///
    ///   TouchAndGoEventMessage
    ///   LandingEventMessage, on Landing
    ///
    /// Exposes:
    ///   Connected, true if connected to MSFS.
    ///
    /// </summary>
    public class SimService : BindableBase, ISimService, INotifyPropertyChanged
    {
        // ms
        private const int SampleRate = 20;

        // flag to ensure only one SimConnect data packet being processed at a time
        private static bool safeToRead = true;

        private static StateMachine stateMachine;

        // timer, task reads data from a SimConnection
        private readonly DispatcherTimer dataReadDispatchTimer = new DispatcherTimer();

        // timer, task establishes connection if disconnected
        private readonly DispatcherTimer connectionDispatchTimer = new DispatcherTimer();

        // Establishes simConnect connection & Update scaneris
        private readonly BackgroundWorker backgroundWorkerConnection = new BackgroundWorker();

        private readonly FsConnect fsConnect = new FsConnect();
        private readonly List<SimVar> definition = new List<SimVar>();

        private int planeInfoDefinitionId;

        private bool running = false;

        private bool connected = false;

        public SimService()
        {
            // do a 'Connection check' every 1 sec
            this.connectionDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            this.connectionDispatchTimer.Tick += new EventHandler(this.ConnectionCheckEventHandler_OnTick);

            // establishes simConnect connection, when required
            this.backgroundWorkerConnection.DoWork += this.BackgroundWorkerConnection_DoWork;
            this.connectionDispatchTimer.Start();

            // Read SimConnect Data every 20 msec
            this.dataReadDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, SampleRate);
            this.dataReadDispatchTimer.Tick += new EventHandler(this.DataReadEventHandler_OnTick);

            // register the read SimConnect data callback procedure
            this.fsConnect.FsDataReceived += HandleReceivedFsData;

            // properties to be read from SimConnect
            this.definition.Add(new SimVar(FsSimVar.Title, null, SIMCONNECT_DATATYPE.STRING256));
            this.definition.Add(new SimVar(FsSimVar.SimOnGround, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32));

            // Wind component in aircraft lateral (X) axis.
            this.definition.Add(new SimVar(FsSimVar.AircraftWindX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));

            // Wind component in aircraft longitudinal(Z) axis.
            this.definition.Add(new SimVar(FsSimVar.AircraftWindZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.AirspeedIndicated, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));

            // Speed relative to the earths surface.
            this.definition.Add(new SimVar(FsSimVar.GroundVelocity, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));

            // lateral speed + to the right
            this.definition.Add(new SimVar(FsSimVar.VelocityBodyX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));

            // speed along airplane axis
            this.definition.Add(new SimVar(FsSimVar.VelocityBodyZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.GForce, FsUnit.GForce, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.PlaneTouchdownNormalVelocity, FsUnit.FeetPerSecond, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.PlaneAltitudeAboveGround, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.PlaneLatitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.PlaneLongitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));

            this.definition.Add(new SimVar(FsSimVar.PlaneBankDegrees, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.OnAnyRunway, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32));
            this.definition.Add(new SimVar(FsSimVar.AtcRunwayAirportName, null, SIMCONNECT_DATATYPE.STRING256));

            // definition.Add(new SimVar(FsSimVar.AtcRunwayRelativePositionX, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            // definition.Add(new SimVar(FsSimVar.AtcRunwayRelativePositionZ, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.AtcRunwaySelected, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32));
            this.definition.Add(new SimVar(FsSimVar.AtcRunwayTdpointRelativePositionX, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));

            // definition.Add(new SimVar(FsSimVar.AtcRunwayTdpointRelativePositionY, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.AtcRunwayTdpointRelativePositionZ, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.RelativeWindVelocityBodyX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.RelativeWindVelocityBodyZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.AmbientWindX, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.AmbientWindZ, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.RelativeWindVelocityBodyY, FsUnit.FeetPerMinute, SIMCONNECT_DATATYPE.FLOAT64));
            this.definition.Add(new SimVar(FsSimVar.GearPosition, FsUnit.Enum, SIMCONNECT_DATATYPE.INT32));
        }

        private event EventHandler<FlightEventArgs> EventHandler;

        private enum Requests
        {
            PlaneInfoRequest = 0,
        }

        public bool Connected
        {
            get { return this.connected; }
            private set { this.connected = value; }
        }

        /// <summary>
        /// Publishes Messages driven by the Event Type & flightParameters.
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Provides the EventType & flightParameters</param>
        protected virtual void FlightEventHandler(object sender, FlightEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            switch (e.EventType())
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
                        var flightParameters = StateMachine.GetMostRecentLandingFlightParameters(e.StateMachine());
                        WeakReferenceMessenger.Default.Send(new TouchAndGoEventMessage(flightParameters));
                    }

                    break;

                case EventType.LandingEvent:
                    Log.Debug("Landing Event");

                    // update & reveil viewModels
                    {
                        var flightParameters = StateMachine.GetMostRecentLandingFlightParameters(e.StateMachine());
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

        private static void HandleReceivedFsData(object sender, FsDataReceivedEventArgs e)
        {
            if (!safeToRead)
            {
                // already processing a packet, skip this one
                Log.Debug("lost one");
                return;
            }

            safeToRead = false;
            try
            {
                if (e.RequestId == (uint)Requests.PlaneInfoRequest)
                {
                    stateMachine.Handle((PlaneInfoResponse)e.Data.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message);
            }

            safeToRead = true;
        }

        private void DataReadEventHandler_OnTick(object sender, EventArgs e)
        {
            try
            {
                this.fsConnect.RequestData((int)Requests.PlaneInfoRequest, this.planeInfoDefinitionId);
            }
            catch
            {
            }
        }

        private void ConnectionCheckEventHandler_OnTick(object sender, EventArgs e)
        {
            if (!this.backgroundWorkerConnection.IsBusy)
            {
                this.backgroundWorkerConnection.RunWorkerAsync();
            }

            bool oldConnected = this.Connected;

            if (this.fsConnect.Connected)
            {
                if (!this.running)
                {
                    this.EventHandler += this.FlightEventHandler;

                    stateMachine = new StateMachine(new ConnectedState(), this.EventHandler);
                    this.running = true;
                    this.dataReadDispatchTimer.Start();
                }

                this.Connected = true;
            }
            else
            {
                this.Connected = false;
            }

            if (this.Connected != oldConnected)
            {
                this.OnPropertyChanged(nameof(this.Connected));
            }
        }

        // If not connected , Connect
        private void BackgroundWorkerConnection_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!this.fsConnect.Connected)
            {
                try
                {
                    // connect & register data of interest
                    this.fsConnect.Connect("TestApp", "localhost", 500, SimConnectProtocol.Ipv4);
                    this.planeInfoDefinitionId = this.fsConnect.RegisterDataDefinition<PlaneInfoResponse>(Requests.PlaneInfoRequest, this.definition);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
