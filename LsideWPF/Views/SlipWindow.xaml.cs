namespace LsideWPF.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for SlipWindow.xaml.
    /// </summary>
    public partial class SlipWindow : Window
    {
        public SlipWindow()
        {
            this.InitializeComponent();
        }

        private void Slip_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
