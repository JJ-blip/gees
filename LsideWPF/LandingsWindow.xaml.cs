using LsideWPF.model;
using System;
using System.Diagnostics;
using System.Windows;

namespace LsideWPF
{
    public partial class LandingsWindow : Window
    {

        public LandingsWindow(LandingViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }

        private void MyLandings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void SaveFolder_Click(object sender, RoutedEventArgs e)
        {
            string myDocs = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Properties.Settings.Default.LandingDirectory;
            string path = myDocs + dir;
            Process.Start(path);
        }
    }
}
