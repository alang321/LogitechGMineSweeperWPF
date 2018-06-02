using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace LogitechGMineSweeper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        #region Class Variables

        public System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();

        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string cls, string win);
        [DllImport("user32")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool OpenIcon(IntPtr hWnd);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static int last = -1;

        //for single instance
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8B}");

        #endregion

        #region Main
        [STAThread]
        public static void Main()
        {
            if (!LogitechGSDK.LogiLedInit()) Console.Write("Not connected to GSDK");

            //Create or read in save files

            Directory.CreateDirectory(Config.directory);

            try
            {
                MineSweeper.UseBackground = Config.fileConfig.UseBackground;
                MineSweeper.Bombs = Config.fileConfig.Bombs;
                MineSweeper.KeyboardLayout = Config.fileConfig.Layout;
            }
            catch
            {
                Config.fileConfig.ResetToDefault();
                MineSweeper.UseBackground = Config.fileConfig.UseBackground;
                MineSweeper.Bombs = Config.fileConfig.Bombs;
                MineSweeper.KeyboardLayout = Config.fileConfig.Layout;
            }

            //one instance code
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                _hookID = SetHook(_proc);
                var application = new App();
                application.Init();
                application.Run(); 
                mutex.ReleaseMutex();
                UnhookWindowsHookEx(_hookID);
            }
        }
        #endregion

        #region Get Keypress and Parse

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
            Debug.WriteLine("Key ID-Code: " + vkCode);
                if (Config.KeyboardLayouts[MineSweeper.KeyboardLayout].KeyIds.Contains(vkCode))
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        if (last != 107 && vkCode == 107) MineSweeper.KeyPressed(48);
                        else if (vkCode != 107)
                        {
                            MineSweeper.SetFlag(Array.IndexOf(Config.KeyboardLayouts[MineSweeper.KeyboardLayout].KeyIds, vkCode));
                            last = -1;
                        }
                    }
                    else if (last != vkCode)
                    {
                        last = vkCode;
                        MineSweeper.KeyPressed(Array.IndexOf(Config.KeyboardLayouts[MineSweeper.KeyboardLayout].KeyIds, vkCode));
                    }
                    else
                    {
                        Debug.WriteLine("REJECTED: Double Press - Key ID-Code: " + vkCode);
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        #endregion

        #region icon

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
            nIcon.Click += nIcon_Click;
        }


        void nIcon_Click(object sender, EventArgs e)
        {
            var mainWnd = System.Windows.Application.Current.MainWindow as MainWindow;
            nIcon.Visible = false;

            if (!mainWnd.IsVisible)
            {
                mainWnd.Show();
            }

            if (mainWnd.WindowState == WindowState.Minimized)
            {
                mainWnd.WindowState = WindowState.Normal;
            }

            mainWnd.Activate();
            mainWnd.Topmost = true;  // important
            mainWnd.Topmost = false; // important
            mainWnd.Focus();

            MineSweeper.printLogiLED();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            nIcon.Visible = false;
            nIcon.Dispose();
        }

        #endregion
    }

    #region One Instance
    internal class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
    #endregion
}