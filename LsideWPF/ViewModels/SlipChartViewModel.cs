namespace LsideWPF.ViewModels
{
    using System;
    using System.Data;
    using System.Linq;
    using LsideWPF.Services;

    public class SlipChartViewModel
    {
        // The path to the csv file of slip data
        private readonly string path;

        public SlipChartViewModel(SlipChartWindow slipChartWindow)
        {
            this.SlipChartWindow = slipChartWindow;

            this.path = this.SlipChartWindow.Path;

            SlipChartService service = new SlipChartService();
            var dt = service.GetLandingLogEntries(this.path);
            this.BuildArrays(dt);
        }

        public SlipChartWindow SlipChartWindow { get; }

        public void BuildArrays(DataTable dt)
        {
            DataRow[] rows = dt.Select();

            /* DateTime[] TimeData = rows.Select(row => row[0].ToString()).ToArray()
            ///      .Select(d => DateTime.Parse(d)).ToArray();
            /// SlipChartWindow.TimeData = new PlotData<DateTime>("Time", TimeData);
            */

            double[] altitude = rows.Select(row => row[1].ToString()).ToArray()
                 .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.Altitude = new PlotData<double>("Altitude", altitude);

            double[] verticalSpeed = rows.Select(row => row[2].ToString()).ToArray()
                 .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.Fpm = new PlotData<double>("Vertical Speed", verticalSpeed);

            double[] airSpeedInd = rows.Select(row => row[3].ToString()).ToArray()
                .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.AirSpeedInd = new PlotData<double>("Air Speed Ind", airSpeedInd);

            double[] groundSpeed = rows.Select(row => row[4].ToString()).ToArray()
                .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.GroundSpeed = new PlotData<double>("Ground Speed", groundSpeed);

            double[] slipAngle = rows.Select(row => row[7].ToString()).ToArray()
                .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.SlipAngle = new PlotData<double>("Slip Angle", slipAngle);

            double[] bankAngle = rows.Select(row => row[8].ToString()).ToArray()
                    .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.BankAngle = new PlotData<double>("Bank Angle", bankAngle);

            double[] driftAngle = rows.Select(row => row[9].ToString()).ToArray()
                .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.DriftAngle = new PlotData<double>("Drift Angle", driftAngle);

            double[] heading = rows.Select(row => row[10].ToString()).ToArray()
                .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.Heading = new PlotData<double>("Heading", heading);

            double[] headWind = rows.Select(row => row[11].ToString()).ToArray()
                .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.HeadWind = new PlotData<double>("Headwind", headWind);

            double[] crossWind = rows.Select(row => row[12].ToString()).ToArray()
                .Select(d => Convert.ToDouble(d)).ToArray();
            this.SlipChartWindow.CrossWind = new PlotData<double>("Crosswind", crossWind);
        }
    }
}
