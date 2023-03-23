using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GeesWPF
{
    /// <summary>
    /// Landing Rate Monitor Display - Top Left Slidable
    /// Interaction logic for LRMDisplay.xaml
    /// </summary>
    public partial class LRMDisplay : Window
    {
        #region Don't ever take focus
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //Set the window style to noactivate.
            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion

        DispatcherTimer timerClose = new DispatcherTimer();

        ViewModel viewModel;

        public LRMDisplay(ViewModel landingModel)
        {
            ;
            this.viewModel = landingModel;
            this.DataContext = viewModel;
            InitializeComponent();
            timerClose.Tick += AutoHide;
        }

        public void AutoHide(object sender, EventArgs e)
        {
            this.BeginStoryboard(FindResource("hide") as Storyboard);
            timerClose.Stop();
        }

        // open & show 
        public void SlideLeft()
        {
            // auto close on 10 s
            timerClose.Interval = new TimeSpan(0, 0, Properties.Settings.Default.CloseAfterLanding);
            if (Properties.Settings.Default.AutoCloseLanding)
            {
                timerClose.Start();
            }
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                // In UI Thread - display current contents
                Storyboard sb = (Storyboard)FindResource("show");
                BeginStoryboard(sb);
            });
        }

        // explicit close request by clicking image1, the red chevron
        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.BeginStoryboard(FindResource("hide") as Storyboard);
        }

        // didplays last LMR view model state, button defined within xaml
        private void button_Click(object sender, RoutedEventArgs e)
        {
            // If Is closed
            if (Width < 350)
            {
                SlideLeft();
            }
        }
    }
}
