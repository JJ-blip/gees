namespace LsideWPF.Views
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using CommunityToolkit.Mvvm.Messaging;
    using LsideWPF.Common;
    using LsideWPF.ViewModels;

    /// <summary>
    ///
    /// Landing Rate Monitor Display - Top Left Slidable
    /// Interaction logic for LRMDisplay.xaml.
    ///
    /// </summary>
    public partial class LRMDisplay : Window
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private readonly DispatcherTimer timerClose = new DispatcherTimer();

        public LRMDisplay()
        {
            this.timerClose.Tick += this.AutoHide;

            WeakReferenceMessenger.Default.Register<LRMDisplay, SlideLeftMessage>(this, (sender, args) =>
            {
                this.SlideLeft();
            });

            this.InitializeComponent();

            // Show as 2 pixel slither (closed) on LHS of user screen
            this.Show();
        }

        public void AutoHide(object sender, EventArgs e)
        {
            if (this.Width > 2)
            {
                // close it , if already open
                this.BeginStoryboard(this.FindResource("hide") as Storyboard);
            }

            this.timerClose.Stop();
        }

        // open & show
        public void SlideLeft()
        {
            // auto close on 10 s
            this.timerClose.Interval = new TimeSpan(0, 0, Properties.Settings.Default.CloseAfterLanding);
            if (Properties.Settings.Default.AutoCloseLanding)
            {
                this.timerClose.Start();
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                // In UI Thread - display current contents
                Storyboard sb = (Storyboard)this.FindResource("show");
                this.BeginStoryboard(sb);
            });
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // extended window style
            int gWL_EXSTYLE = -20;

            // do not take focus
            int wS_EX_NOACTIVATE = 0x08000000;

            // Set the window style to noactivate.
            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, gWL_EXSTYLE,
                    GetWindowLong(helper.Handle, gWL_EXSTYLE) | wS_EX_NOACTIVATE);
        }

        // explicit close request by clicking image1, the red chevron
        private void Image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.BeginStoryboard(this.FindResource("hide") as Storyboard);
        }

        // displays last LMR view model state,overlayButton lies on left sidewall, enables user to open on a click
        private void OverlayButton_Click(object sender, RoutedEventArgs e)
        {
            // If Is closed, then open it
            if (this.Width < 350)
            {
                this.SlideLeft();
            }
        }
    }
}