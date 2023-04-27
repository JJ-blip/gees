namespace LsideWPF.Views
{
    using System.Windows;
    using LsideWPF.ViewModels;

    /// <summary>
    /// Interaction logic for SlipWindow.xaml.
    /// </summary>
    public partial class SlipWindow : Window
    {
        private static SlipChartWindow openGraphWindow = null;

        // The path to the csv file of slip data
        private readonly string path;

        public SlipWindow()
        {
            // initilization via the ViewModels
            this.InitializeComponent();
            this.path = ((SlipViewModel)this.DataContext).FullFileName;
        }

        public SlipWindow(string path)
        {
            // Initilize the DataContext
            this.InitializeComponent();

            // but immediately reload explicitly via a path
            ((SlipViewModel)this.DataContext).Path = path;
            this.path = path;
        }

        private void Slip_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void GraphSlip_Click(object sender, RoutedEventArgs re)
        {
            if (openGraphWindow == null)
            {
                // create window & let it do its thing.
                openGraphWindow = new SlipChartWindow(this.path);
                openGraphWindow.Show();
                openGraphWindow.Closed += (s, e) => openGraphWindow = null;
            }
            else
            {
                openGraphWindow.Focus();
            }
        }
    }
}
