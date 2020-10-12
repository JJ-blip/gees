﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CTrue.FsConnect;
using Microsoft.FlightSimulator.SimConnect;
using System.Linq;
using System.Diagnostics;
using Octokit;
using FontAwesome.Sharp;
using System.Threading;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Concurrent;

namespace Gees
{
    public enum Requests
    {
        PlaneInfo = 0
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneInfoResponse
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Type;
        public bool OnGround;
        public double WindLat;
        public double WindHead;
        public double AirspeedInd;
        public double GroundSpeed;
        public double LateralSpeed;
        public double ForwardSpeed;
        public double Gforce;
        public double Radio;
    }

    public partial class FormMain : Form
    {
        #region Exception Handlers and Logging
        static public void UnhandledThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            HandleUnhandledException(e.Exception);
        }

        static public void HandleUnhandledException(Exception e)
        {
            MessageBox.Show(e.Message);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string logout = "\n\n" + DateTime.Now.ToString() + "\n" + System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion + "\n" +
                e.Message + "\n" + e.Source + "\n" + e.StackTrace;
            System.IO.File.AppendAllText(@"./log.txt", logout);
        }

        #endregion

        #region Publics and statics
        static bool ShowLanding = false;
        static bool SafeToRead = true;
        static List<PlaneInfoResponse> Inair = new List<PlaneInfoResponse>();
        static List<PlaneInfoResponse> Onground = new List<PlaneInfoResponse>();
        static FsConnect fsConnect = new FsConnect();
        static List<SimProperty> definition = new List<SimProperty>();
        static string updateUri;
        int lastDeactivateTick;
        bool lastDeactivateValid;

        const int SAMPLE_RATE = 20; //ms
        const int BUFFER_SIZE = 10;
        #endregion

        public FormMain()
        {
            InitializeComponent();
            MakeLogIfEmpty();
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            labelVersion.Text = fvi.FileVersion;

            timerRead.Interval = SAMPLE_RATE;
            button1.Select();
            iconGithub.IconChar = IconChar.Github;
            iconReddit.IconChar = IconChar.Reddit;
            iconFAS.IconChar = IconChar.FontAwesome;

            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width - 20,
                                      workingArea.Bottom - Size.Height - 20);
            backgroundWorkerUpdate.RunWorkerAsync();
            fsConnect.FsDataReceived += HandleReceivedFsData;
            definition.Add(new SimProperty("TITLE", null, SIMCONNECT_DATATYPE.STRING256));
            definition.Add(new SimProperty("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32));
            definition.Add(new SimProperty("AIRCRAFT WIND X", "Knots", SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty("AIRCRAFT WIND Z", "Knots", SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty("GROUND VELOCITY", "Knots", SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty("VELOCITY BODY X", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty("VELOCITY BODY Z", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty("G FORCE", "GForce", SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty("PLANE ALT ABOVE GROUND", "Feet", SIMCONNECT_DATATYPE.FLOAT64));
      
        }

        #region Reading and processing the Simconnect data
        private void timerRead_Tick(object sender, EventArgs e)
        {
            if (!ShowLanding)
            {
                try
                {
                    fsConnect.RequestData(Requests.PlaneInfo);
                }
                catch
                {
                }
            }
            else
            {
                timerWait.Start();
            }
        }

        private static void HandleReceivedFsData(object sender, FsDataReceivedEventArgs e)
        {
            if (!SafeToRead)
            {
                Console.WriteLine("lost one");
                return;
            }
            SafeToRead = false;
            try
            {
                if (e.RequestId == (uint)Requests.PlaneInfo)
                {
                    if (!ShowLanding)
                    {
                        PlaneInfoResponse r = (PlaneInfoResponse)e.Data;
                        //ignore when noone is flying
                        if (r.GroundSpeed == 0)
                        {
                            SafeToRead = true;
                            return;
                        }
                        if (r.OnGround)
                        {
                            Onground.Add(r);
                            if (Onground.Count > BUFFER_SIZE)
                            {
                                Onground.RemoveAt(0);
                                if (Inair.Count == BUFFER_SIZE)
                                {
                                    ShowLanding = true;
                                }
                            }
                        }
                        else
                        {
                            Inair.Add(r);
                            if (Inair.Count > BUFFER_SIZE)
                            {
                                Inair.RemoveAt(0);
                            }
                            Onground.Clear();
                        }
                        if (Inair.Count > BUFFER_SIZE || Onground.Count > BUFFER_SIZE) //maximum 1 for race condition
                        {
                            Inair.Clear();
                            Onground.Clear();
                            throw new Exception("this baaad");
                        }
                        // POnGround = r.OnGround;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            SafeToRead = true;
        }

        private void timerWait_Tick(object sender, EventArgs e)
        {
            //impact calculation
            try
            {
                double sample_time = Convert.ToDouble(SAMPLE_RATE) * 0.001; //ms
                double fpm = 60 * (Inair.ElementAt(BUFFER_SIZE / 2).Radio - Onground.ElementAt(BUFFER_SIZE / 2).Radio) / (sample_time * Convert.ToDouble(BUFFER_SIZE));
                Int32 FPM = Convert.ToInt32(-fpm);

                double gees = 0;
                int Gforcemeterlen = 100 / SAMPLE_RATE; // take 100ms average for G force
                for (int i = 0; i < Gforcemeterlen; i++)
                {
                    gees += Onground.ElementAt(i).Gforce;
                }
                gees /= Gforcemeterlen;

                double incAngle = Math.Atan(Inair.Last().LateralSpeed / Inair.Last().ForwardSpeed) * 180 / Math.PI;

                EnterLog(Inair.First().Type, FPM, gees, Inair.Last().AirspeedInd, Inair.Last().GroundSpeed, Inair.Last().WindHead, Inair.Last().WindLat, incAngle);
                LRMDisplay form = new LRMDisplay(FPM, gees, Inair.Last().AirspeedInd, Inair.Last().GroundSpeed, Inair.Last().WindHead, Inair.Last().WindLat, incAngle);
                form.Show();
                timerWait.Stop();
                Inair.Clear();
                Onground.Clear();
                ShowLanding = false;
            }
            catch
            {
                //some params are missing. likely the user is in the main menu. ignore
            }
        }
        #endregion

        #region AutoConnection to sim
        private void timerConnection_Tick(object sender, EventArgs e)
        {
            if (!backgroundConnector.IsBusy)
                backgroundConnector.RunWorkerAsync();

            if (fsConnect.Connected)
            {
                timerRead.Start();
                iconConnStatus.BackgroundImage = Properties.Resources.connected;
                labelConn.Text = "Connected";
                this.Icon = Properties.Resources.online;
                notifyIcon.Icon = Properties.Resources.online;
            }
            else
            {
                iconConnStatus.BackgroundImage = Properties.Resources.disconnected;
                labelConn.Text = "Disconnected";
                this.Icon = Properties.Resources.offline;
                notifyIcon.Icon = Properties.Resources.offline;
            }
        }

        private void backgroundConnector_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!fsConnect.Connected)
            {
                try
                {
                    fsConnect.Connect("TestApp", "localhost", 500);
                    fsConnect.RegisterDataDefinition<PlaneInfoResponse>(Requests.PlaneInfo, definition);
                }
                catch { } // ignore
            }
        }
        #endregion

        #region Form Events and buttons/links

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Properties.Settings.Default.Save();
            Environment.Exit(1);
        }

        private void button1_Click(object sender, EventArgs e) //testing testing
        {
            /*int[] k = { 2, 3 };
            int i = k[10]; //duuuuh*/
            LRMDisplay form = new LRMDisplay(-100, 1.03, 65, 67, -5, -5, -2.34);
            form.Show();
        }
        private void iconReddit_LinkClicked(object sender, EventArgs e)
        {
            Process.Start("https://www.reddit.com/r/MSFS2020LandingRate/");
        }

        private void iconGithub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/scelts/msfslandingrate");
        }

        private void iconFAS_Click(object sender, EventArgs e)
        {
            Process.Start("https://fontawesome.com/");
        }

        private void buttonLandings_Click(object sender, EventArgs e)
        {
            FormHistory form1 = new FormHistory();
            form1.Show();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoClose = checkBox1.Checked;
            Properties.Settings.Default.Save();
            if (!checkBox1.Checked)
            {
                numericUpDown1.Enabled = false;
            }
            else
            {
                numericUpDown1.Enabled = true;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CloseAfter = numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }
        #endregion


        #region Update checks from github
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            Process.Start(updateUri);
        }

        private void backgroundWorkerUpdate_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var client = new GitHubClient(new ProductHeaderValue("Gees"));
            var releases = client.Repository.Release.GetAll("scelts", "gees").Result;
            var latest = releases[0];
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            if (version != latest.TagName)
            {
                e.Result = latest;
            }
            else
            {
                e.Result = null;
            }
        }

        private void backgroundWorkerUpdate_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                buttonUpdate.Text = "Connection error";
                //  MessageBox.Show(e.Error.Message);
                //error handler, tbd
            }
            else
            {
                if (e.Result != null)
                {
                    buttonUpdate.Text = "Updates Available";
                    buttonUpdate.BackColor = Color.FromArgb(230, 57, 70);
                    buttonUpdate.Enabled = true;
                    updateUri = (e.Result as Release).HtmlUrl;
                }
            }
        }
        #endregion

        #region Logging and data handling
        void MakeLogIfEmpty()
        {
            const string header = "Time,Plane,FPM,Impact (G),Air Speed (kt),Ground Speed (kt),Headwind (kt),Crosswind (kt),Sideslip (deg)";
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Directory.CreateDirectory(myDocs + @"\MyMSFS2020Landings-Gees"); //create if doesn't exist
            string path = myDocs + @"\MyMSFS2020Landings-Gees\Landings.v1.csv";
            if (!File.Exists(path))
            {
                using (StreamWriter w = File.CreateText(path))
                {
                    w.WriteLine(header);
                }
            }
        }
        void EnterLog(string Plane, int FPM, double G, double airV, double groundV, double headW, double crossW, double sideslip)
        {
            MakeLogIfEmpty();
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = myDocs + @"\MyMSFS2020Landings-Gees\Landings.v1.csv";
            using (StreamWriter w = File.AppendText(path))
            {
                string logLine = DateTime.Now.ToString("G") + ",";
                logLine += Plane + ",";
                logLine += FPM + ",";
                logLine += G.ToString("0.##") + ",";
                logLine += airV.ToString("0.##") + ",";
                logLine += groundV.ToString("0.##") + ",";
                logLine += headW.ToString("0.##") + ",";
                logLine += crossW.ToString("0.##") + ",";
                logLine += sideslip.ToString("0.##");
                w.WriteLine(logLine);
            }
        }
        #endregion

        #region Eye candies, show/hide/minimise tray

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            lastDeactivateTick = Environment.TickCount;
            lastDeactivateValid = true;
            this.Hide();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                AnimateWindow(this.Handle, 200, AW_BLEND);
            }
            else
            {
                AnimateWindow(this.Handle, 200, AW_BLEND | AW_HIDE);
            }
            base.OnVisibleChanged(e);
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (lastDeactivateValid && Environment.TickCount - lastDeactivateTick < 1000) return;
            this.Show();
            this.Activate();
        }

        const int AW_SLIDE = 0X40000;

        const int AW_VER_POSITIVE = 0X4;

        const int AW_VER_NEGATIVE = 0X8;

        const int AW_HIDE = 65536;
        const int AW_BLEND = 0x00080000;

        [DllImport("user32")]

        static extern bool AnimateWindow(IntPtr hwnd, int time, int flags);

        #endregion

    }
}

