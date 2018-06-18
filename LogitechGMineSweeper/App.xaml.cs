using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.IO;

namespace LogitechGMineSweeper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        
        //for single instance
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8B}");

        [STAThread]
        public static void Main()
        {
            LogitechGSDK.LogiLedInit();

            //one instance code
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                var application = new App();
                application.Init();
                application.Run(); 
                mutex.ReleaseMutex();
            }
        }
        public void Init()
        {
            this.InitializeComponent();
        }

        public App()
        {
            String dir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string root = Directory.GetParent(dir).FullName;
            nIcon.Icon = new Icon(Path.Combine(root, "icon.ico"));
            nIcon.Visible = false;
            nIcon.Click += NIcon_Click;
        }

        void NIcon_Click(object sender, EventArgs e)
        {
            MainWindow mainWnd = System.Windows.Application.Current.MainWindow as MainWindow;
            nIcon.Visible = false;

            if (!mainWnd.IsVisible)
            {
                mainWnd.Show();
            }

            if(mainWnd._menuTabControl.SelectedIndex == 1)
            {
                mainWnd.KeyboardDisplayShown = true;
            }

            if (mainWnd.WindowState == WindowState.Minimized)
            {
                mainWnd.WindowState = WindowState.Normal;
            }

            mainWnd.Activate();
            mainWnd.Topmost = true;  // important
            mainWnd.Topmost = false; // important
            mainWnd.Focus();

            mainWnd.MineSweeper.PrintLogiLED();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            nIcon.Visible = false;
            nIcon.Dispose();
        }
    }
}