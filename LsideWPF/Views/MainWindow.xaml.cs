namespace LsideWPF.Views
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using LsideWPF.Services;
    using LsideWPF.ViewModels;
    using Octokit;
    using Serilog;
    using MouseEventArgs = System.Windows.Input.MouseEventArgs;
    using MouseEventHandler = System.Windows.Input.MouseEventHandler;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string updateUri;

        private static Mutex mutex;

        private static LandingsWindow openLandingsWindow = null;
        private static SlipWindow openLastSlipWindow = null;

        // Background Git updated
        private readonly BackgroundWorker backgroundWorkerUpdate = new BackgroundWorker();

        private readonly ViewModel viewModel;

        // main window - drag & drop
        private bool mouseDown;

        public MainWindow()
        {
            bool createdNew = true;

            mutex = new Mutex(true, "Lside", out createdNew);
            if (!createdNew)
            {
                System.Windows.MessageBox.Show("This application is already running.", "Lside", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            Log.Logger = new LoggerConfiguration()
                     .WriteTo.Console()
                     .MinimumLevel.Debug()
                     .Enrich.FromLogContext()

                     // CTrue log activity, some still escapes
                     .Filter.ByExcluding(logevent => logevent.MessageTemplate.Text.Contains("Recv"))
                     .CreateLogger();

            Log.Debug("started");

            this.viewModel = new ViewModels.ViewModel();

            ViewModel viewModel1 = this.viewModel;
            this.DataContext = viewModel1;
            this.InitializeComponent();

            // Desktop POSITION
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;

            // Git Hub Updater, runs once immediately
            this.backgroundWorkerUpdate.DoWork += this.BackgroundWorkerUpdate_DoWork;
            this.backgroundWorkerUpdate.RunWorkerAsync();

            // create child window(s) - that know how to look after themselves
            LRMDisplay winLRM = new LRMDisplay();
        }

        /** MainWindow drag and drop  **/
        private void Header_LoadedHandler(object sender, RoutedEventArgs e)
        {
            this.InitHeader(sender as TextBlock);
        }

        private void InitHeader(TextBlock header)
        {
            header.MouseUp += new MouseButtonEventHandler(this.OnMouseUp);
            header.MouseLeftButtonDown += new MouseButtonEventHandler(this.MouseLeftButtonDown);
            header.MouseMove += new MouseEventHandler(this.OnMouseMove);
        }

        private new void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.mouseDown = true;
            }

            this.DragMove();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.mouseDown = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseDown)
            {
                var mouseX = e.GetPosition(this).X;
                var width = this.RestoreBounds.Width;
                var x = mouseX - (width / 2);

                if (x < 0)
                {
                    x = 0;
                }
                else
                if (x + width > SystemParameters.PrimaryScreenWidth)
                {
                    x = SystemParameters.PrimaryScreenWidth - width;
                }

                this.WindowState = WindowState.Normal;
                this.Left = x;
                this.Top = 0;
                this.DragMove();
            }
        }

        /* Handlers for UI */

        private void Button_Hide_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void RedditLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.reddit.com/r/MSFS2020LandingRate/");
        }

        private void GithubLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/scelts/lside");
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(updateUri);
        }

        private void ButtonLandings_Click(object sender, RoutedEventArgs re)
        {
            if (openLandingsWindow == null)
            {
                // create window & let it do its thing.
                openLandingsWindow = new LandingsWindow();
                openLandingsWindow.Show();
                openLandingsWindow.Closed += (s, e) => openLandingsWindow = null;
            }
            else
            {
                openLandingsWindow.Focus();
            }
        }

        private void ButtonShowLastSlip_Click(object sender, RoutedEventArgs re)
        {
            if (openLastSlipWindow == null)
            {
                // create window & let it do its thing.
                openLastSlipWindow = new SlipWindow();
                openLastSlipWindow.Show();
                openLastSlipWindow.Closed += (s, e) => openLastSlipWindow = null;
            }
            else
            {
                openLastSlipWindow.Focus();
            }
        }

        private void ButtonBrowseAllSlip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            FileService fileService = new FileService();

            openFileDialog.InitialDirectory = fileService.GetBasePath();
            openFileDialog.Filter = "All files (SlipLog-*.csv)|SlipLog-*.csv";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var filename = openFileDialog.FileName;

                // create window & let it do its thing.
                SlipWindow slipWindow = new SlipWindow(filename);

                slipWindow.Show();
            }
        }

        private void ButtonShowLastLanding_Click(object sender, RoutedEventArgs e)
        {
            // refresh model & displays it
            WeakReferenceMessenger.Default.Send<ShowLastLandingMessage>();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(this.textBox.Text, out _))
            {
                Properties.Settings.Default.Save();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out _))
            {
            }
            else
            {
                e.Handled = true;
            }
        }

        private void ComboBoxScreens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        /** Git Hub Updater, amends displayed URL in the Main Window. **/
        private void BackgroundWorkerUpdate_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var client = new GitHubClient(new ProductHeaderValue("Lside"));
            var releases = client.Repository.Release.GetAll("jj-blip", "lside").Result;
            var latest = releases[0];
            var tagVersion = latest.TagName.Remove(0, 1);

            var currentVersion = new Version(this.viewModel.Version);
            var latestGitVersion = new Version(tagVersion);
            var versionDifference = latestGitVersion.CompareTo(currentVersion);

            this.viewModel.UpdateAvailable = versionDifference > 0;
            updateUri = latest.HtmlUrl;
        }

        private void ButtonSlipOn_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SlipOn(true);
        }

        private void ButtonSlipOff_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SlipOn(false);
        }
    }
}
