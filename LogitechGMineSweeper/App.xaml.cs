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

        public static string[] colors = { "000,000,000", "000,127,255", "255,255,000", "000,128,000", "000,255,255", "128,000,064", "255,000,000", "000,000,255", "255,255,255", "255,200,200", "000,000,255", "255,000,000", "000,000,255", "000,255,255", "255,160,160", "000,255,255" };


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
            string[] lines = { "Wins: 0", "Bombs: 13", "Layout: DE", "Total: 0", "Losses: 0" };
            string[] US = { "", "", "", "", "", "5: 30:00", "6: 30:00", "7: 30:00", "8: 30:00", "9: 30:00", "10: 30:00", "11: 30:00", "12: 30:00", "13: 30:00", "14: 30:00", "15: 30:00", "16: 30:00", "17: 30:00", "18: 30:00", "19: 30:00", "20: 30:00", "21: 30:00", "22: 30:00", "23: 30:00", "24: 30:00", "25: 30:00", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };

            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var directory = Path.Combine(systemPath, "Logitech MineSweeper");

            var file = Path.Combine(systemPath, "Logitech MineSweeper/config.txt");
            var fileUS = Path.Combine(systemPath, "Logitech MineSweeper/US.txt");
            var fileDE = Path.Combine(systemPath, "Logitech MineSweeper/DE.txt");
            var fileUK = Path.Combine(systemPath, "Logitech MineSweeper/UK.txt");
            var fileColors = Path.Combine(systemPath, "Logitech MineSweeper/colors.txt");


            Directory.CreateDirectory(directory);

            int wins = 0;
            int bombs = 0;
            int total = 0;
            int losses = 0;
            string layout = "";

            if (!File.Exists(fileUS))
            {
                File.WriteAllLines(fileUS, US);
            }
            if (!File.Exists(fileDE))
            {
                File.WriteAllLines(fileDE, US);
            }
            if (!File.Exists(fileUK))
            {
                File.WriteAllLines(fileUK, US);
            }
            if (!File.Exists(fileColors))
            {
                File.WriteAllLines(fileColors, colors);
            }

            try
            {
                for (int i = 0; i < MineSweeper.colors.GetLength(0); i++)
                {
                    MineSweeper.colors[i, 0] = Convert.ToByte(File.ReadLines(fileColors).Skip(i).Take(1).First().Substring(0, 3));
                    MineSweeper.colors[i, 1] = Convert.ToByte(File.ReadLines(fileColors).Skip(i).Take(1).First().Substring(4, 3));
                    MineSweeper.colors[i, 2] = Convert.ToByte(File.ReadLines(fileColors).Skip(i).Take(1).First().Substring(8, 3));
                }
            }
            catch
            {
                File.WriteAllLines(fileColors, colors);
            }


            if (File.Exists(file))
            {
                string line1 = File.ReadLines(file).Skip(0).Take(1).First();
                string line2 = File.ReadLines(file).Skip(1).Take(1).First();
                string line3 = File.ReadLines(file).Skip(2).Take(1).First();
                string line4 = File.ReadLines(file).Skip(3).Take(1).First();
                string line5 = File.ReadLines(file).Skip(4).Take(1).First();

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

                    a = line3.IndexOf("Layout: ");
                    layout = line3.Substring(a + "Layout: ".Length);

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
                    bombs = 13;
                    layout = "DE";
                    total = 0;
                    losses = 0;
                    File.WriteAllLines(file, lines);
                    newFile = true;
                }

                if (!newFile)
                {
                    if (wins >= 0 && bombs >= 5 && bombs <= 25)
                    {
                        if (layout == "US" || layout == "DE" || layout == "UK")
                        {

                        }
                        else
                        {
                            layout = "DE";
                            File.WriteAllLines(file, lines);
                            newFile = true;
                        }
                    }
                    else
                    {
                        wins = 0;
                        bombs = 13;
                        total = 0;
                        File.WriteAllLines(file, lines);
                        newFile = true;
                    }
                }
            }
            else
            {
                wins = 0;
                bombs = 13;
                layout = "DE";
                total = 0;
                losses = 0;
                File.WriteAllLines(file, lines);
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

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string key = Convert.ToString((Keys)vkCode);
                if (key == "D1" || key == "D2" || key == "D3" || key == "D4" || key == "D5" || key == "D6" || key == "D7" || key == "D8" || key == "D9" || key == "D0" || key == "OemOpenBrackets" || key == "Q" || key == "W" || key == "E" || key == "R" || key == "T" || key == "Z" || key == "U" || key == "I" || key == "O" || key == "P" || key == "Oem1" || key == "A" || key == "S" || key == "D" || key == "F" || key == "G" || key == "H" || key == "J" || key == "K" || key == "L" || key == "Oemtilde" || key == "Oem7" || key == "OemBackslash" || key == "Y" || key == "X" || key == "C" || key == "V" || key == "B" || key == "N" || key == "M" || key == "Oemcomma" || key == "OemPeriod" || key == "OemMinus" || key == "Add" || key == "OemQuestion" || key == "Oem5")
                {
                    if (MineSweeper.KeyboardLayout == "US" && (key == "OemBackslash" || key == "Oem5"))
                    {
                    }
                    else if (MineSweeper.KeyboardLayout == "DE" && (key == "OemQuestion" || key == "Oemtilde" || key == "Oem5"))
                    {
                    }
                    else if (MineSweeper.KeyboardLayout == "UK" && (key == "OemBackslash" || key == "Oem7"))
                    {
                    }
                    else if (Control.ModifierKeys == Keys.Shift)
                    {
                        AssignParameter(key);
                        if (last != "Add" && key == "Add") MineSweeper.keyPressed(99);
                        else if (key != "Add")
                        {
                            MineSweeper.SetFlag(parameter - 1);
                            last = "empty";
                        }
                        Console.WriteLine("Shift - " + key);

                    }
                    else if (last != key)
                    {
                        last = key;
                        AssignParameter(key);

                        MineSweeper.keyPressed(parameter - 1);

                        Console.WriteLine(key);

                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }



        private static void AssignParameter(string key)
        {
            if(MineSweeper.KeyboardLayout == "DE")
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
                    default: Console.WriteLine("DEFAULT"); break;
                }
            }
            else if(MineSweeper.KeyboardLayout == "US")
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
                    case "OemOpenBrackets": parameter = 22; break;
                    case "Q": parameter = 12; break;
                    case "W": parameter = 13; break;
                    case "E": parameter = 14; break;
                    case "R": parameter = 15; break;
                    case "T": parameter = 16; break;
                    case "Z": parameter = 35; break;
                    case "U": parameter = 18; break;
                    case "I": parameter = 19; break;
                    case "O": parameter = 20; break;
                    case "P": parameter = 21; break;
                    case "Oem1": parameter = 32; break;
                    case "A": parameter = 23; break;
                    case "S": parameter = 24; break;
                    case "D": parameter = 25; break;
                    case "F": parameter = 26; break;
                    case "G": parameter = 27; break;
                    case "H": parameter = 28; break;
                    case "J": parameter = 29; break;
                    case "K": parameter = 30; break;
                    case "L": parameter = 31; break;
                    case "Oem7": parameter = 33; break;
                    case "Y": parameter = 17; break;
                    case "X": parameter = 36; break;
                    case "C": parameter = 37; break;
                    case "V": parameter = 38; break;
                    case "B": parameter = 39; break;
                    case "N": parameter = 40; break;
                    case "M": parameter = 41; break;
                    case "Oemcomma": parameter = 42; break;
                    case "OemPeriod": parameter = 43; break;
                    case "OemQuestion": parameter = 44; break;
                    case "OemMinus": parameter = 11; break;
                    case "Add": parameter = 100; break;
                    default: Console.WriteLine("DEFAULT"); break;
                }
            }
            else if (MineSweeper.KeyboardLayout == "UK")
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
                    case "OemOpenBrackets": parameter = 22; break;
                    case "Q": parameter = 12; break;
                    case "W": parameter = 13; break;
                    case "E": parameter = 14; break;
                    case "R": parameter = 15; break;
                    case "T": parameter = 16; break;
                    case "Z": parameter = 35; break;
                    case "U": parameter = 18; break;
                    case "I": parameter = 19; break;
                    case "O": parameter = 20; break;
                    case "P": parameter = 21; break;
                    case "Oemtilde": parameter = 33; break;
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
                    case "Oem5": parameter = 34; break;
                    case "Y": parameter = 17; break;
                    case "X": parameter = 36; break;
                    case "C": parameter = 37; break;
                    case "V": parameter = 38; break;
                    case "B": parameter = 39; break;
                    case "N": parameter = 40; break;
                    case "M": parameter = 41; break;
                    case "Oemcomma": parameter = 42; break;
                    case "OemPeriod": parameter = 43; break;
                    case "OemQuestion": parameter = 44; break;
                    case "OemMinus": parameter = 11; break;
                    case "Add": parameter = 100; break;
                    default: Console.WriteLine("DEFAULT"); break;
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
