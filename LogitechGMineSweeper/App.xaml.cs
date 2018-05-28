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
            bool newFile = false;

            Directory.CreateDirectory(Config.directory);

            int wins = 0;
            int bombs = 0;
            int total = 0;
            int losses = 0;
            int layout = 0;
            bool useBackground = Config.useBackgroundDefault; ;

            foreach (KeyboardLayout keyLayout in Config.KeyboardLayouts)
            {
                if (!File.Exists(keyLayout.SaveFile))
                {
                    File.WriteAllLines(keyLayout.SaveFile, Config.statisticsDefault);
                }
            }

            if (!File.Exists(Config.fileColors))
            {
                File.WriteAllLines(Config.fileColors, Config.colorsDefault);
            }

            try
            {
                for (int i = 0; i < MineSweeper.colors.GetLength(0); i++)
                {
                    MineSweeper.colors[i, 0] = Convert.ToByte(File.ReadLines(Config.fileColors).Skip(i).Take(1).First().Substring(0, 3));
                    MineSweeper.colors[i, 1] = Convert.ToByte(File.ReadLines(Config.fileColors).Skip(i).Take(1).First().Substring(4, 3));
                    MineSweeper.colors[i, 2] = Convert.ToByte(File.ReadLines(Config.fileColors).Skip(i).Take(1).First().Substring(8, 3));
                }
            }
            catch
            {
                File.WriteAllLines(Config.fileColors, Config.colorsDefault);
                for (int i = 0; i < MineSweeper.colors.GetLength(0); i++)
                {
                    MineSweeper.colors[i, 0] = Convert.ToByte(File.ReadLines(Config.fileColors).Skip(i).Take(1).First().Substring(0, 3));
                    MineSweeper.colors[i, 1] = Convert.ToByte(File.ReadLines(Config.fileColors).Skip(i).Take(1).First().Substring(4, 3));
                    MineSweeper.colors[i, 2] = Convert.ToByte(File.ReadLines(Config.fileColors).Skip(i).Take(1).First().Substring(8, 3));
                }
            }


            if (File.Exists(Config.fileConfig))
            {
                string line1 = File.ReadLines(Config.fileConfig).Skip(0).Take(1).First();
                string line2 = File.ReadLines(Config.fileConfig).Skip(1).Take(1).First();
                string line3 = File.ReadLines(Config.fileConfig).Skip(2).Take(1).First();
                string line4 = File.ReadLines(Config.fileConfig).Skip(3).Take(1).First();
                string line5 = File.ReadLines(Config.fileConfig).Skip(4).Take(1).First();

                int a = 0;
                string b = "";

                try
                {
                    a = line1.IndexOf("Wins: ");
                    b = line1.Substring(a + "Wins: ".Length);
                    wins = Convert.ToInt32(b);

                    a = line2.IndexOf("Bombs: ");
                    b = line2.Substring(a + "Bombs: ".Length);
                    bombs = Convert.ToInt32(b);

                    a = line4.IndexOf("Total: ");
                    b = line4.Substring(a + "Total: ".Length);
                    total = Convert.ToInt32(b);

                    a = line5.IndexOf("Losses: ");
                    b = line5.Substring(a + "Losses: ".Length);
                    losses = Convert.ToInt32(b);
                }
                catch
                {
                    wins = 0;
                    bombs = Config.bombsDefault;
                    layout = Config.keyboardLayoutDefault;
                    total = 0;
                    losses = 0;
                    useBackground = Config.useBackgroundDefault;
                    File.WriteAllLines(Config.fileConfig, Config.configDefault);
                    newFile = true;
                }

                try
                {
                    a = line3.IndexOf("Layout: ");
                    b = line3.Substring(a + "Layout: ".Length);
                    layout = Convert.ToInt32(b);
                }
                catch
                {
                    string[] lines = { "Wins: " + wins, "Bombs: " + bombs, "Layout: " + Config.keyboardLayoutDefault, "Total: " + total, "Losses: " + losses, "UseBackground: False" };

                    File.WriteAllLines(Config.fileConfig, lines);
                    layout = Config.keyboardLayoutDefault;
                }

                try
                {
                    string line6 = File.ReadLines(Config.fileConfig).Skip(5).Take(1).First();
                    a = line6.IndexOf("UseBackground: ");
                    b = line6.Substring(a + "UseBackground: ".Length);
                    if(b == "False")
                    {
                        useBackground = false;
                    }
                    else if(b == "True")
                    {
                        useBackground = true;
                    }
                    else
                    {
                        throw new Exception("Invalid Value for Use Background in config.txt");
                    }
                }
                catch
                {
                    string[] lines = { "Wins: " + wins, "Bombs: " + bombs, "Layout: " + layout, "Total: " + total, "Losses: " + losses, "UseBackground: False"};

                    File.WriteAllLines(Config.fileConfig, lines);
                    layout = Config.keyboardLayoutDefault;
                }


                if (!newFile)
                {
                    if (wins >= 0 && bombs >= 5 && bombs <= 25)
                    {
                        if (layout == (int)Config.Layout.US || layout == (int)Config.Layout.DE || layout == (int)Config.Layout.UK)
                        {

                        }
                        else
                        {
                            layout = Config.keyboardLayoutDefault;
                            File.WriteAllLines(Config.fileConfig, Config.configDefault);
                            newFile = true;
                        }
                    }
                    else
                    {
                        wins = 0;
                        bombs = Config.bombsDefault;
                        total = 0;
                        File.WriteAllLines(Config.fileConfig, Config.configDefault);
                        newFile = true;
                    }
                }
            }
            else
            {
                wins = 0;
                bombs = Config.bombsDefault;
                layout = Config.keyboardLayoutDefault;
                total = 0;
                losses = 0;
                useBackground = Config.useBackgroundDefault;
                File.WriteAllLines(Config.fileConfig, Config.configDefault);
                newFile = true;
            }
            
            MineSweeper.useBackground = useBackground;
            MineSweeper.Wins = wins;
            MineSweeper.Total = total;
            MineSweeper.Bombs = bombs;
            MineSweeper.Losses = losses;
            MineSweeper.KeyboardLayout = layout;

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
            else
            {
                // send our Win32 message to make the currently running instance
                // jump on top of all the other windows
                NativeMethods.PostMessage(
                (IntPtr)NativeMethods.HWND_BROADCAST,
                NativeMethods.WM_SHOWME,
                IntPtr.Zero,
                IntPtr.Zero);
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
                if (Config.KeyboardLayouts[MineSweeper.KeyboardLayout].KeyIds.Contains(vkCode))
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        if (last != 107 && vkCode == 107) MineSweeper.keyPressed(48);
                        else if (vkCode != 107)
                        {
                            MineSweeper.SetFlag(Array.IndexOf(Config.KeyboardLayouts[MineSweeper.KeyboardLayout].KeyIds, vkCode));
                            last = -1;
                        }
                        Debug.WriteLine("Key ID-Code: Shift - " + vkCode);
                    }
                    else if (last != vkCode)
                    {
                        last = vkCode;
                        MineSweeper.keyPressed(Array.IndexOf(Config.KeyboardLayouts[MineSweeper.KeyboardLayout].KeyIds, vkCode));

                        Debug.WriteLine("Key ID-Code: " + vkCode);
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