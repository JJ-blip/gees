using LsideWPF.Models;
using LsideWPF.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace LsideWPF.Views
{
    public partial class LandingsWindow : Window
    {

        private ILandingLoggerService landingLogger = App.Current.Services.GetService<ILandingLoggerService>();

        public LandingsWindow()
        {
            InitializeComponent();
        }

        private void MyLandings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void SaveFolder_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView items = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            landingLogger.Add(items.Cast<LogEntry>().ToList());
        }

        private void UngroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView items = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (items != null)
            {
                items.GroupDescriptions.Clear();
            }
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView items = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (items != null && items.CanGroup == true)
            {
                items.GroupDescriptions.Clear();
                items.GroupDescriptions.Add(new PropertyGroupDescription("Plane"));
                items.GroupDescriptions.Add(new PropertyGroupDescription("Airport"));
            }
        }

        private void CompleteFilter_Changed(object sender, RoutedEventArgs e)
        {
            // Refresh the view to apply filters.
            CollectionViewSource.GetDefaultView(dataGrid.ItemsSource).Refresh();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            LogEntry entry = e.Item as LogEntry;
            if (entry != null)
            {
                if (this.textSearch == null || entry.Plane.Contains(this.textSearch.Text))
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
        }
    }
}
