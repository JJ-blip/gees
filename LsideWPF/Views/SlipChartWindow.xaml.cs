namespace LsideWPF
{
    using System.Windows;
    using LsideWPF.ViewModels;
    using ScottPlot;
    using ScottPlot.Plottable;

    /// <summary>
    /// The ScottPlot library is not MVVM compliant. Its used in full acknowledgment of that.
    /// The View objects are fully exposed to the model via Service's
    /// The view model is non existent!
    ///
    /// Interaction logic for SlipChartWindow.xaml.
    /// </summary>
    public partial class SlipChartWindow : Window
    {
        private readonly ScatterPlot fpmPlot;
        private readonly ScatterPlot airSpeedIndPlot;
        private readonly ScatterPlot groundSpeedPlot;
        private readonly ScatterPlot headWindPlot;
        private readonly ScatterPlot crossWindPlot;
        private readonly ScatterPlot slipAnglePlot;
        private readonly ScatterPlot bankPlot;
        private readonly ScatterPlot driftPlot;
        private readonly ScatterPlot headingPlot;

        private readonly ScottPlot.Renderable.Axis yAxis2;
        private readonly ScottPlot.Renderable.Axis yAxis3;
        private readonly ScottPlot.Renderable.Axis yAxis4;
        private readonly ScottPlot.Renderable.Axis yAxis5;
        private readonly ScottPlot.Renderable.Axis yAxis6;

        public SlipChartWindow(string path)
        {
            this.InitializeComponent();

            // SlipChartViewModel will update this objects fields with SlipData to plot
            // Data is esentially static
            this.Path = path;
            new SlipChartViewModel(this);

            var plt = this.wpfPlot.Plot;

            plt.YLabel(this.Altitude.Title);
            plt.XLabel($"Degrees");

            this.slipAnglePlot = this.AddScatter(plt, this.SlipAngle, this.Altitude);
            this.slipAnglePlot.XAxisIndex = 0;

            this.bankPlot = this.AddScatter(plt, this.BankAngle, this.Altitude);
            this.bankPlot.XAxisIndex = 0;
            this.bankPlot.MarkerShape = MarkerShape.asterisk;
            this.driftPlot = this.AddScatter(plt, this.DriftAngle, this.Altitude);
            this.driftPlot.XAxisIndex = 0;
            this.driftPlot.MarkerShape = MarkerShape.hashTag;

            this.yAxis2 = this.wpfPlot.Plot.AddAxis(ScottPlot.Renderable.Edge.Bottom, 2, "FPM");
            this.fpmPlot = this.AddScatter(plt, this.Fpm, this.Altitude);
            this.yAxis2.Color(this.fpmPlot.Color);
            this.fpmPlot.MarkerShape = MarkerShape.triDown;
            this.fpmPlot.XAxisIndex = 2;
            this.fpmPlot.YAxisIndex = 0;

            this.yAxis3 = this.wpfPlot.Plot.AddAxis(ScottPlot.Renderable.Edge.Bottom, 3, "Aircraft Speed (Kts)");
            this.airSpeedIndPlot = this.AddScatter(plt, this.AirSpeedInd, this.Altitude);
            this.airSpeedIndPlot.MarkerShape = MarkerShape.openTriangleUp;
            this.airSpeedIndPlot.XAxisIndex = 3;
            this.airSpeedIndPlot.YAxisIndex = 0;
            this.groundSpeedPlot = this.AddScatter(plt, this.GroundSpeed, this.Altitude);
            this.groundSpeedPlot.MarkerShape = MarkerShape.openTriangleDown;
            this.groundSpeedPlot.XAxisIndex = 3;
            this.groundSpeedPlot.YAxisIndex = 0;

            this.yAxis4 = this.wpfPlot.Plot.AddAxis(ScottPlot.Renderable.Edge.Bottom, 4, "Headwind (Kts)");
            this.headWindPlot = this.AddScatter(plt, this.HeadWind, this.Altitude);
            this.yAxis4.Color(this.headWindPlot.Color);
            this.yAxis4.MinimumTickSpacing(1);
            this.headWindPlot.MarkerShape = MarkerShape.verticalBar;
            this.headWindPlot.XAxisIndex = 4;
            this.headWindPlot.YAxisIndex = 0;

            this.yAxis5 = this.wpfPlot.Plot.AddAxis(ScottPlot.Renderable.Edge.Bottom, 5, "Heading");
            this.headingPlot = this.AddScatter(plt, this.Heading, this.Altitude);
            this.yAxis5.Color(this.headingPlot.Color);
            this.headingPlot.MarkerShape = MarkerShape.openCircle;
            this.headingPlot.XAxisIndex = 5;
            this.headingPlot.YAxisIndex = 0;

            this.yAxis6 = this.wpfPlot.Plot.AddAxis(ScottPlot.Renderable.Edge.Bottom, 6, "Crosswind (Kts)");
            this.crossWindPlot = this.AddScatter(plt, this.CrossWind, this.Altitude);
            this.yAxis6.Color(this.crossWindPlot.Color);
            this.yAxis6.MinimumTickSpacing(1);
            this.crossWindPlot.MarkerShape = MarkerShape.cross;
            this.crossWindPlot.XAxisIndex = 6;
            this.crossWindPlot.YAxisIndex = 0;

            plt.Legend();

            // refreshes all on.
            this.wpfPlot.Plot.AxisAuto();
            this.wpfPlot.Refresh();

            // read the checkboxes and display plots as appropriate.
            if (this.cb1.IsChecked == false)
            {
                this.AircraftSpeedHide(this, null);
            }

            if (this.cb2.IsChecked == false)
            {
                this.HeadwindHide(this, null);
            }

            if (this.cb3.IsChecked == false)
            {
                this.FpmHide(this, null);
            }

            if (this.cb4.IsChecked == false)
            {
                this.SlipsHide(this, null);
            }

            if (this.cb5.IsChecked == false)
            {
                this.BankHide(this, null);
            }

            if (this.cb6.IsChecked == false)
            {
                this.DriftHide(this, null);
            }

            if (this.cb7.IsChecked == false)
            {
                this.HeadingHide(this, null);
            }

            if (this.cb8.IsChecked == false)
            {
                this.CrosswindHide(this, null);
            }
        }

        public PlotData<double> Fpm { get; internal set; }

        public PlotData<double> Altitude { get; internal set; }

        public PlotData<double> AirSpeedInd { get; internal set; }

        public PlotData<double> GroundSpeed { get; internal set; }

        public PlotData<double> HeadWind { get; internal set; }

        public PlotData<double> CrossWind { get; internal set; }

        public PlotData<double> SlipAngle { get; internal set; }

        public PlotData<double> BankAngle { get; internal set; }

        public PlotData<double> DriftAngle { get; internal set; }

        public PlotData<double> Heading { get; internal set; }

        public string Path { get; internal set; }

        private ScatterPlot AddScatter(Plot plot, PlotData<double> xPlotData, PlotData<double> yPlotData)
        {
            return plot.AddScatter(xPlotData.Data, yPlotData.Data, label: xPlotData.Title);
        }

        private void AllOff(object sender, RoutedEventArgs e)
        {
            this.cb1.IsChecked = false;
            this.cb2.IsChecked = false;
            this.cb3.IsChecked = false;
            this.cb4.IsChecked = false;
            this.cb5.IsChecked = false;
            this.cb6.IsChecked = false;
            this.cb7.IsChecked = false;
            this.cb8.IsChecked = false;
        }

        private void AircraftSpeedHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.airSpeedIndPlot.IsVisible = false;
            this.groundSpeedPlot.IsVisible = false;
            this.yAxis3.Hide(true);
            this.yAxis3.Label(string.Empty);
            this.wpfPlot.Refresh();
        }

        private void AircraftSpeedShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.airSpeedIndPlot.IsVisible = true;
            this.groundSpeedPlot.IsVisible = true;
            this.yAxis3.Hide(false);
            this.yAxis3.Label("Aircraft Speed (kts)");
            this.wpfPlot.Refresh();
        }

        private void HeadwindHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.headWindPlot.IsVisible = false;
            this.yAxis4.Hide(true);
            this.yAxis4.Label(string.Empty);
            this.wpfPlot.Refresh();
        }

        private void HeadwindShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.headWindPlot.IsVisible = true;
            this.yAxis4.Hide(false);
            this.yAxis4.Label("Headwind (kts)");
            this.wpfPlot.Refresh();
        }

        private void CrosswindHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.crossWindPlot.IsVisible = false;
            this.yAxis6.Hide(true);
            this.yAxis6.Label(string.Empty);
            this.wpfPlot.Refresh();
        }

        private void CrosswindShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.crossWindPlot.IsVisible = true;
            this.yAxis6.Hide(false);
            this.yAxis6.Label("Crosswind (kts)");
            this.wpfPlot.Refresh();
        }

        private void FpmHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.fpmPlot.IsVisible = false;
            this.yAxis2.Hide(true);
            this.yAxis2.Label(string.Empty);
            this.wpfPlot.Refresh();
        }

        private void FpmShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.fpmPlot.IsVisible = true;
            this.yAxis2.Hide(false);
            this.yAxis2.Label("FPM");
            this.wpfPlot.Refresh();
        }

        private void SlipsHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.slipAnglePlot.IsVisible = false;
            this.ShowHideDegrees();
            this.wpfPlot.Refresh();
        }

        private void SlipsShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.slipAnglePlot.IsVisible = true;
            this.ShowHideDegrees();
            this.wpfPlot.Refresh();
        }

        private void BankHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.bankPlot.IsVisible = false;
            this.ShowHideDegrees();
            this.wpfPlot.Refresh();
        }

        private void BankShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.bankPlot.IsVisible = true;
            this.ShowHideDegrees();
            this.wpfPlot.Refresh();
        }

        private void DriftHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.driftPlot.IsVisible = false;
            this.ShowHideDegrees();
            this.wpfPlot.Refresh();
        }

        private void DriftShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.driftPlot.IsVisible = true;
            this.ShowHideDegrees();
            this.wpfPlot.Refresh();
        }

        private void HeadingHide(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.headingPlot.IsVisible = false;
            this.yAxis5.Hide(true);
            this.yAxis5.Label(string.Empty);
            this.wpfPlot.Refresh();
        }

        private void HeadingShow(object sender, RoutedEventArgs e)
        {
            if (this.wpfPlot is null)
            {
                return;
            }

            this.headingPlot.IsVisible = true;
            this.yAxis5.Hide(false);
            this.yAxis5.Label("Heading");
            this.wpfPlot.Refresh();
        }

        private void ShowHideDegrees()
        {
            if (this.driftPlot.IsVisible || this.bankPlot.IsVisible || this.slipAnglePlot.IsVisible)
            {
                this.wpfPlot.Plot.XAxis.Hide(false);
                this.wpfPlot.Plot.XAxis.Label("Degrees");
            }
            else
            {
                this.wpfPlot.Plot.XAxis.Hide(true);
                this.wpfPlot.Plot.XAxis.Label(string.Empty);
            }
        }
    }
}
