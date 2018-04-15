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

        public static string last = "empty";
        private static int parameter = 0;


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

            foreach(string file in Config.fileStatistics)
            {
                if (!File.Exists(file))
                {
                    File.WriteAllLines(file, Config.statisticsDefault);
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
                    layout = Config.keyboardLayout;
                    total = 0;
                    losses = 0;
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
                    string[] lines = { "Wins: " + wins, "Bombs: " + bombs, "Layout: " + Config.keyboardLayout, "Total: " + total, "Losses: " + losses };

                    File.WriteAllLines(Config.fileConfig, lines);
                    layout = Config.keyboardLayout;
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
                            layout = Config.keyboardLayout;
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
                layout = Config.keyboardLayout;
                total = 0;
                losses = 0;
                File.WriteAllLines(Config.fileConfig, Config.configDefault);
                newFile = true;
            }

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

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string key = Convert.ToString((Keys)vkCode);
                if (key == "D1" || key == "D2" || key == "D3" || key == "D4" || key == "D5" || key == "D6" || key == "D7" || key == "D8" || key == "D9" || key == "D0" || key == "OemOpenBrackets" || key == "Q" || key == "W" || key == "E" || key == "R" || key == "T" || key == "Z" || key == "U" || key == "I" || key == "O" || key == "P" || key == "Oem1" || key == "A" || key == "S" || key == "D" || key == "F" || key == "G" || key == "H" || key == "J" || key == "K" || key == "L" || key == "Oemtilde" || key == "Oem7" || key == "OemBackslash" || key == "Y" || key == "X" || key == "C" || key == "V" || key == "B" || key == "N" || key == "M" || key == "Oemcomma" || key == "OemPeriod" || key == "OemMinus" || key == "Add" || key == "OemQuestion" || key == "Oem5")
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        AssignParameter(key);
                        if (parameter != 99999)
                        {
                            if (last != "Add" && key == "Add") MineSweeper.keyPressed(99);
                            else if (key != "Add")
                            {
                                MineSweeper.SetFlag(parameter - 1);
                                last = "empty";
                            }
                            Console.WriteLine("Shift - " + key);
                        }
                    }
                    else if (last != key)
                    {
                        last = key;
                        AssignParameter(key);
                        if(parameter != 99999)
                        {
                            MineSweeper.keyPressed(parameter - 1);
                        }

                        Console.WriteLine(key);
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }



        private static void AssignParameter(string key)
        {
            Console.WriteLine(MineSweeper.KeyboardLayout + "<<<<<<<<<<------------------  HEYYYYYYYYY");
            if(MineSweeper.KeyboardLayout == (int)Config.Layout.DE)
            {
                switch (key)
                {
                    case "D1": parameter = 1; break;
                    case "D2": parameter = 2; break;
                    case "D3": parameter = 3; break;
                    case "D4": parameter = 4; break;
                    case "D5": parameter = 5; break;
                    case "D6": parameter = 6; break;
                    case "D7": parameter = 7; break;
                    case "D8": parameter = 8; break;
                    case "D9": parameter = 9; break;
                    case "D0": parameter = 10; break;
                    case "OemOpenBrackets": parameter = 11; break;
                    case "Q": parameter = 12; break;
                    case "W": parameter = 13; break;
                    case "E": parameter = 14; break;
                    case "R": parameter = 15; break;
                    case "T": parameter = 16; break;
                    case "Z": parameter = 17; break;
                    case "U": parameter = 18; break;
                    case "I": parameter = 19; break;
                    case "O": parameter = 20; break;
                    case "P": parameter = 21; break;
                    case "Oem1": parameter = 22; break;
                    case "A": parameter = 23; break;
                    case "S": parameter = 24; break;
                    case "D": parameter = 25; break;
                    case "F": parameter = 26; break;
                    case "G": parameter = 27; break;
                    case "H": parameter = 28; break;
                    case "J": parameter = 29; break;
                    case "K": parameter = 30; break;
                    case "L": parameter = 31; break;
                    case "Oemtilde": parameter = 32; break;
                    case "Oem7": parameter = 33; break;
                    case "OemBackslash": parameter = 34; break;
                    case "Y": parameter = 35; break;
                    case "X": parameter = 36; break;
                    case "C": parameter = 37; break;
                    case "V": parameter = 38; break;
                    case "B": parameter = 39; break;
                    case "N": parameter = 40; break;
                    case "M": parameter = 41; break;
                    case "Oemcomma": parameter = 42; break;
                    case "OemPeriod": parameter = 43; break;
                    case "OemMinus": parameter = 44; break;
                    case "Add": parameter = 100; break;
                    default: Console.WriteLine("DEFAULT"); parameter = 99999; break;
                }
            }
            else if(MineSweeper.KeyboardLayout == (int)Config.Layout.US)
            {
                switch (key)
                {
                    case "D1": parameter = 1; break;
                    case "D2": parameter = 2; break;
                    case "D3": parameter = 3; break;
                    case "D4": parameter = 4; break;
                    case "D5": parameter = 5; break;
                    case "D6": parameter = 6; break;
                    case "D7": parameter = 7; break;
                    case "D8": parameter = 8; break;
                    case "D9": parameter = 9; break;
                    case "D0": parameter = 10; break;
                    case "OemMinus": parameter = 11; break;
                    case "Q": parameter = 12; break;
                    case "W": parameter = 13; break;
                    case "E": parameter = 14; break;
                    case "R": parameter = 15; break;
                    case "T": parameter = 16; break;
                    case "Y": parameter = 17; break;
                    case "U": parameter = 18; break;
                    case "I": parameter = 19; break;
                    case "O": parameter = 20; break;
                    case "P": parameter = 21; break;
                    case "OemOpenBrackets": parameter = 22; break;
                    case "A": parameter = 23; break;
                    case "S": parameter = 24; break;
                    case "D": parameter = 25; break;
                    case "F": parameter = 26; break;
                    case "G": parameter = 27; break;
                    case "H": parameter = 28; break;
                    case "J": parameter = 29; break;
                    case "K": parameter = 30; break;
                    case "L": parameter = 31; break;
                    case "Oem1": parameter = 32; break;
                    case "Oem7": parameter = 33; break;
                    //no 34 as that key is not present in us keyboards
                    case "Z": parameter = 35; break;
                    case "X": parameter = 36; break;
                    case "C": parameter = 37; break;
                    case "V": parameter = 38; break;
                    case "B": parameter = 39; break;
                    case "N": parameter = 40; break;
                    case "M": parameter = 41; break;
                    case "Oemcomma": parameter = 42; break;
                    case "OemPeriod": parameter = 43; break;
                    case "OemQuestion": parameter = 44; break;
                    case "Add": parameter = 100; break;
                    default: Console.WriteLine("DEFAULT"); parameter = 99999; break;
                }
            }
            else if (MineSweeper.KeyboardLayout == (int)Config.Layout.UK)
            {
                switch (key)
                {
                    case "D1": parameter = 1; break;
                    case "D2": parameter = 2; break;
                    case "D3": parameter = 3; break;
                    case "D4": parameter = 4; break;
                    case "D5": parameter = 5; break;
                    case "D6": parameter = 6; break;
                    case "D7": parameter = 7; break;
                    case "D8": parameter = 8; break;
                    case "D9": parameter = 9; break;
                    case "D0": parameter = 10; break;
                    case "OemMinus": parameter = 11; break;
                    case "Q": parameter = 12; break;
                    case "W": parameter = 13; break;
                    case "E": parameter = 14; break;
                    case "R": parameter = 15; break;
                    case "T": parameter = 16; break;
                    case "Y": parameter = 17; break;
                    case "U": parameter = 18; break;
                    case "I": parameter = 19; break;
                    case "O": parameter = 20; break;
                    case "P": parameter = 21; break;
                    case "OemOpenBrackets": parameter = 22; break;
                    case "A": parameter = 23; break;
                    case "S": parameter = 24; break;
                    case "D": parameter = 25; break;
                    case "F": parameter = 26; break;
                    case "G": parameter = 27; break;
                    case "H": parameter = 28; break;
                    case "J": parameter = 29; break;
                    case "K": parameter = 30; break;
                    case "L": parameter = 31; break;
                    case "Oem1": parameter = 32; break;
                    case "Oemtilde": parameter = 33; break;
                    case "Oem5": parameter = 34; break;
                    case "Z": parameter = 35; break;
                    case "X": parameter = 36; break;
                    case "C": parameter = 37; break;
                    case "V": parameter = 38; break;
                    case "B": parameter = 39; break;
                    case "N": parameter = 40; break;
                    case "M": parameter = 41; break;
                    case "Oemcomma": parameter = 42; break;
                    case "OemPeriod": parameter = 43; break;
                    case "OemQuestion": parameter = 44; break;
                    case "Add": parameter = 100; break;
                    default: Console.WriteLine("DEFAULT"); parameter = 99999; break;
                }
            }
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
