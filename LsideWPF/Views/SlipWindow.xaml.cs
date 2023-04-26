namespace LsideWPF.Views
{
    using System.Windows;
    using LsideWPF.ViewModels;

    /// <summary>
    /// Interaction logic for SlipWindow.xaml.
    /// </summary>
    public partial class SlipWindow : Window
    {
        public SlipWindow()
        {
            this.InitializeComponent();
        }

        public SlipWindow(string path)
            : this()
        {
            ((SlipViewModel)this.DataContext).Path = path;
        }

        private void Slip_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
