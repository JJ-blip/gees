using LsideWPF.model;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace LsideWPF
{
    public partial class LandingsWindow : Window
    {

        public LandingsWindow(LandingViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();

            // Get a reference to the tasks collection.
            LogEntries logEntries = (LogEntries)this.Resources["logEntries"];

            LandingLogger logger = new LandingLogger();
            var _logEntries = LandingLogger.GetLandingLogEntries();

            foreach (var logEntry in _logEntries)
            {
                logEntries.Add(logEntry);
            }
        }

        private void MyLandings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void SaveFolder_Click(object sender, RoutedEventArgs e)
        {
            LandingLogger logger = new LandingLogger();
            ICollectionView items = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            logger.LogItems(items.Cast<LogEntry>().ToList());
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
                if (entry.Plane.Contains(this.textSearch.Text))
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
        }
    }
}
