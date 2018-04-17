using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Shapes;


namespace LogitechGMineSweeper
{
    #region blur2
    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    #endregion

    public partial class MainWindow : Window
    {
        #region constructor and variables

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public static System.Timers.Timer dispatcherTimer = new System.Timers.Timer();
        private static readonly Stopwatch timer = new Stopwatch();

        //default brush of the keyboard button foreground so you dont need to type it out
        public static SolidColorBrush a = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFC, 0xFC, 0xFC));

        //for changing color of hovereffect when key color changes in game
        public bool hovering = false;
        public System.Windows.Shapes.Rectangle toFill;
        public System.Windows.Shapes.Rectangle fromFill;

        public MainWindow()
        {
            InitializeComponent();

            //norammly wpf application maximize over taskbar, fix from LesterLobo https://blogs.msdn.microsoft.com/llobo/2006/08/01/maximizing-window-with-windowstylenone-considering-taskbar/
            //diesnt work perfectly with multiple monitors
            win.SourceInitialized += new EventHandler(win_SourceInitialized);
            //

            win.SizeChanged += new SizeChangedEventHandler(SizeChangedWin);

            _menuTabControl.SelectedIndex = 0;
            MineSweeper.main = App.Current.MainWindow as MainWindow;

            dispatcherTimer.Elapsed += new ElapsedEventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = 1000;
            dispatcherTimer.Stop();
            
            UpdateTimer();

            KeyLayout.SelectedIndex = MineSweeper.KeyboardLayout;

            NUDTextBox.Text = Convert.ToString(MineSweeper.Bombs);

            UpdateColors();
            UpdateStats();

            MineSweeper.newGame();
        }

        #endregion

        #region maximize not over taskbar

        void win_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            System.Windows.Interop.HwndSource.FromHwnd(handle).AddHook(new System.Windows.Interop.HwndSourceHook(WindowProc));
        }


        //public override void OnApplyTemplate()
        //{
        //    System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
        //    WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        //}

        private static System.IntPtr WindowProc(
              System.IntPtr hwnd,
              int msg,
              System.IntPtr wParam,
              System.IntPtr lParam,
              ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {

            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }


        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };


        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }


        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }


        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        #endregion
        
        #region blur

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        #endregion

        #region Timer

        string BestTime(int bombs)
        {
            string best = "";
            int a = 0;

            var file = Config.fileStatistics[MineSweeper.KeyboardLayout];

            try
            {
                string skip = bombs + ": ";
                string line = File.ReadLines(file).Skip(bombs).Take(1).First();
                a = line.IndexOf(skip);
                best = line.Substring(a + skip.Length);
                int min = Convert.ToInt32(best.Substring(0, 2));
                int sek = Convert.ToInt32(best.Substring(3, 2));
            }
            catch
            {
                File.WriteAllLines(file, Config.statisticsDefault);
            }
            if (best.Length != 5 || best.Substring(2, 1) != ":" || Convert.ToInt32(best.Substring(0, 2)) > 30 || Convert.ToInt32(best.Substring(3, 2)) > 60 || Convert.ToInt32(best.Substring(0, 2)) < 0 || Convert.ToInt32(best.Substring(3, 2)) < 0)
            {
                File.WriteAllLines(file, Config.statisticsDefault);
            }

            return best;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (timer.Elapsed.Minutes >= 30)
            {
                MineSweeper.newGame();
                StopWatchDefeat();
                ResetWatch();
            }

            // Updating the Label which displays the current second
            UpdateTimer();

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public void StopWatchVictory()
        {
            timer1.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Green);
            timer.Stop();
            dispatcherTimer.Enabled = false;
            UpdateTimer();

            string best = BestTime(MineSweeper.Bombs);

            if (Convert.ToInt32(best.Substring(0, 2)) * 60 + Convert.ToInt32(best.Substring(3, 2)) >= timer.Elapsed.Minutes * 60 + timer.Elapsed.Seconds)
            {
                var file = Config.fileStatistics[MineSweeper.KeyboardLayout];

                string[] updatedStatistics = File.ReadAllLines(file);

                updatedStatistics[MineSweeper.Bombs] = MineSweeper.Bombs.ToString() + ": " + string.Format("{0:00}:{1:00}", timer.Elapsed.Minutes, timer.Elapsed.Seconds);

                File.WriteAllLines(file, updatedStatistics);

                UpdateStats();
            }
        }

        private void UpdateTimer()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                timer1.Content = GetTimeString(timer.Elapsed);
            });
        }

        public void StopWatchDefeat()
        {
            timer1.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
            timer.Stop();
            dispatcherTimer.Enabled = false;
        }

        public void StartWatch()
        {
            timer1.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Black);
            timer.Reset();
            dispatcherTimer.Enabled = true;
            timer.Start();
        }

        public void ResetWatch()
        {
            timer1.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Black);
            timer.Reset();
            UpdateTimer();
        }

        private string GetTimeString(TimeSpan elapsed)
        {
            string result = string.Empty;

            result = string.Format("{0:00}:{1:00}",
                elapsed.Minutes,
                elapsed.Seconds);

            return result;
        }

        #endregion

        #region minimize, close and drag window

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //DragMove();
        }

        private void image1_MouseEnter(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/closeWhite.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void image1_MouseLeave(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/close.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void Stack_mousedown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SizeChangedWin(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if(win.Width < 685)
            {
                win.Width = 685;
            }
            if (win.Height < 394)
            {
                win.Height = 394;
            }

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            ((App)System.Windows.Application.Current).nIcon.Visible = true;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Numeric Up-Down

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number < Config.maxBombs)
                NUDTextBox.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number > Config.minBombs)
                NUDTextBox.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox.Text != "")
                if (!int.TryParse(NUDTextBox.Text, out number)) NUDTextBox.Text = Config.NumUDstartvalue.ToString();
            if (number > Config.maxBombs) NUDTextBox.Text = Config.maxBombs.ToString();
            if (number < Config.minBombs) NUDTextBox.Text = Config.minBombs.ToString();
            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;
            MineSweeper.Bombs = Convert.ToInt32(NUDTextBox.Text);
            MineSweeper.newGame();

            StopWatchDefeat();
            ResetWatch();

            UpdateFile();
            UpdateStats();
        }

        #endregion

        #region Nav-Buttons

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 0;
            var style = new Style();

            style.Setters.Add(new Setter(TemplateProperty, this.FindResource("buttonactive")));

            var styleClone = new Style();
            foreach (var setter in style.Setters)
            {
                var typedSetter = setter as Setter;
                if (typedSetter != null)
                {
                    var newSetter = new Setter(typedSetter.Property, typedSetter.Value);
                    styleClone.Setters.Add(newSetter);
                }
            }
            SetAllButtonsNormal();
            settings.Style = styleClone;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 1;
            var style = new Style();

            style.Setters.Add(new Setter(TemplateProperty, this.FindResource("buttonactive")));

            var styleClone = new Style();
            foreach (var setter in style.Setters)
            {
                var typedSetter = setter as Setter;
                if (typedSetter != null)
                {
                    var newSetter = new Setter(typedSetter.Property, typedSetter.Value);
                    styleClone.Setters.Add(newSetter);
                }
            }
            SetAllButtonsNormal();
            colors.Style = styleClone;

            FlagUseBackground.IsChecked = MineSweeper.useBackground;

            //for switching keyboard styles
            if (MineSweeper.KeyboardLayout == (int)Config.Layout.US)
            {
                rect782.Width = 120;
                rect788.Width = 107;
                g800.Visibility = Visibility.Hidden;
                us1.Visibility = Visibility.Visible;
                us2.Visibility = Visibility.Visible;
                xy.Content = "Z";
                yx.Content = "Y";
                ß.Visibility = Visibility.Hidden;
                ß1.Visibility = Visibility.Visible;
                ü.Visibility = Visibility.Hidden;
                ü1.Visibility = Visibility.Visible;
                ö.Visibility = Visibility.Hidden;
                ö1.Visibility = Visibility.Visible;
                ä.Visibility = Visibility.Hidden;
                ä1.Visibility = Visibility.Visible;
                comma.Visibility = Visibility.Hidden;
                comma1.Visibility = Visibility.Visible;
                dot.Visibility = Visibility.Hidden;
                dot1.Visibility = Visibility.Visible;
                hyphen.Visibility = Visibility.Hidden;
                hyphen1.Visibility = Visibility.Visible;
                enter.Visibility = Visibility.Hidden;
                usenter.Visibility = Visibility.Visible;
                buttonnexttoleftshiftlabel1.Visibility = Visibility.Visible;
                buttonnexttoleftshiftlabel2.Visibility = Visibility.Hidden;
                ä1.Visibility = Visibility.Visible;
                ä2.Visibility = Visibility.Hidden;
                h12newus.Visibility = Visibility.Visible;
                h23newus.Visibility = Visibility.Visible;
                h12newde.Visibility = Visibility.Hidden;
                h23newde.Visibility = Visibility.Hidden;
                showus.Visibility = Visibility.Visible;
            }
            else if (MineSweeper.KeyboardLayout == (int)Config.Layout.DE)
            {
                rect782.Width = 65.5;
                rect788.Width = 53.5;
                g800.Visibility = Visibility.Visible;
                us1.Visibility = Visibility.Hidden;
                us2.Visibility = Visibility.Hidden;
                xy.Content = "Y";
                yx.Content = "Z";
                ß.Visibility = Visibility.Visible;
                ß1.Visibility = Visibility.Hidden;
                ü.Visibility = Visibility.Visible;
                ü1.Visibility = Visibility.Hidden;
                ö.Visibility = Visibility.Visible;
                ö1.Visibility = Visibility.Hidden;
                ä.Visibility = Visibility.Visible;
                ä1.Visibility = Visibility.Hidden;
                comma.Visibility = Visibility.Visible;
                comma1.Visibility = Visibility.Hidden;
                dot.Visibility = Visibility.Visible;
                dot1.Visibility = Visibility.Hidden;
                hyphen.Visibility = Visibility.Visible;
                hyphen1.Visibility = Visibility.Hidden;
                enter.Visibility = Visibility.Visible;
                usenter.Visibility = Visibility.Hidden;
                buttonnexttoleftshiftlabel1.Visibility = Visibility.Visible;
                buttonnexttoleftshiftlabel2.Visibility = Visibility.Hidden;
                ä1.Visibility = Visibility.Visible;
                ä2.Visibility = Visibility.Hidden;
                h12newus.Visibility = Visibility.Hidden;
                h23newus.Visibility = Visibility.Hidden;
                h34newuk.Visibility = Visibility.Hidden;
                h12newde.Visibility = Visibility.Visible;
                h23newde.Visibility = Visibility.Visible;
                h34newde.Visibility = Visibility.Visible;
                showus.Visibility = Visibility.Hidden;
            }
            else if(MineSweeper.KeyboardLayout == (int)Config.Layout.UK)
            {
                rect782.Width = 65.5;
                rect788.Width = 53.5;
                g800.Visibility = Visibility.Visible;
                us1.Visibility = Visibility.Hidden;
                us2.Visibility = Visibility.Hidden;
                xy.Content = "Z";
                yx.Content = "Y";
                ß.Visibility = Visibility.Hidden;
                ß1.Visibility = Visibility.Visible;
                ü.Visibility = Visibility.Hidden;
                ü1.Visibility = Visibility.Visible;
                ö.Visibility = Visibility.Hidden;
                ö1.Visibility = Visibility.Visible;
                ä.Visibility = Visibility.Hidden;
                ä1.Visibility = Visibility.Visible;
                comma.Visibility = Visibility.Hidden;
                comma1.Visibility = Visibility.Visible;
                dot.Visibility = Visibility.Hidden;
                dot1.Visibility = Visibility.Visible;
                hyphen.Visibility = Visibility.Hidden;
                hyphen1.Visibility = Visibility.Visible;
                enter.Visibility = Visibility.Visible;
                usenter.Visibility = Visibility.Hidden;
                buttonnexttoleftshiftlabel1.Visibility = Visibility.Hidden;
                buttonnexttoleftshiftlabel2.Visibility = Visibility.Visible;
                ä1.Visibility = Visibility.Hidden;
                ä2.Visibility = Visibility.Visible;
                h12newus.Visibility = Visibility.Visible;
                h23newus.Visibility = Visibility.Visible;
                h34newuk.Visibility = Visibility.Visible;
                h12newde.Visibility = Visibility.Hidden;
                h23newde.Visibility = Visibility.Hidden;
                h34newde.Visibility = Visibility.Hidden;
                showus.Visibility = Visibility.Hidden;
            }

            UpdateColors();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 2;
            var style = new Style();

            style.Setters.Add(new Setter(TemplateProperty, this.FindResource("buttonactive")));

            var styleClone = new Style();
            foreach (var setter in style.Setters)
            {
                var typedSetter = setter as Setter;
                if (typedSetter != null)
                {
                    var newSetter = new Setter(typedSetter.Property, typedSetter.Value);
                    styleClone.Setters.Add(newSetter);
                }
            }
            SetAllButtonsNormal();
            stats.Style = styleClone;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 3;
            var style = new Style();

            style.Setters.Add(new Setter(TemplateProperty, this.FindResource("buttonactive")));

            var styleClone = new Style();
            foreach (var setter in style.Setters)
            {
                var typedSetter = setter as Setter;
                if (typedSetter != null)
                {
                    var newSetter = new Setter(typedSetter.Property, typedSetter.Value);
                    styleClone.Setters.Add(newSetter);
                }
            }
            SetAllButtonsNormal();
            reset.Style = styleClone;
        }

        private void SetAllButtonsNormal()
        {
            var style = new Style();

            style.Setters.Add(new Setter(TemplateProperty, this.FindResource("button")));

            var styleClone = new Style();
            foreach (var setter in style.Setters)
            {
                var typedSetter = setter as Setter;
                if (typedSetter != null)
                {
                    var newSetter = new Setter(typedSetter.Property, typedSetter.Value);
                    styleClone.Setters.Add(newSetter);
                }
            }

            settings.Style = styleClone;
            stats.Style = styleClone;
            reset.Style = styleClone;
            colors.Style = styleClone;
        }

        private void KeyLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MineSweeper.KeyboardLayout = KeyLayout.SelectedIndex;
            MineSweeper.newGame();

            StopWatchDefeat();
            ResetWatch();

            UpdateFile();
            UpdateStats();
        }

        #endregion

        #region Update statistics and color and file

        public void UpdateColors()
        {
            //Color of border
            esc1.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,0,0,0));

            esc1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[9, 2], MineSweeper.colors[9, 1], MineSweeper.colors[9, 0]));
            zero1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[0, 2], MineSweeper.colors[0, 1], MineSweeper.colors[0, 0]));
            one1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[1, 2], MineSweeper.colors[1, 1], MineSweeper.colors[1, 0]));
            two1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[2, 2], MineSweeper.colors[2, 1], MineSweeper.colors[2, 0]));
            three1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[3, 2], MineSweeper.colors[3, 1], MineSweeper.colors[3, 0]));
            four1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[4, 2], MineSweeper.colors[4, 1], MineSweeper.colors[4, 0]));
            five1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[5, 2], MineSweeper.colors[5, 1], MineSweeper.colors[5, 0]));
            six1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[6, 2], MineSweeper.colors[6, 1], MineSweeper.colors[6, 0]));
            newGame1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[11, 2], MineSweeper.colors[11, 1], MineSweeper.colors[11, 0]));
            d1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[15, 2], MineSweeper.colors[15, 1], MineSweeper.colors[15, 0]));

            Zero.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[0, 2], MineSweeper.colors[0, 1], MineSweeper.colors[0, 0]));
            One.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[1, 2], MineSweeper.colors[1, 1], MineSweeper.colors[1, 0]));
            Two.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[2, 2], MineSweeper.colors[2, 1], MineSweeper.colors[2, 0]));
            Three.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[3, 2], MineSweeper.colors[3, 1], MineSweeper.colors[3, 0]));
            Four.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[4, 2], MineSweeper.colors[4, 1], MineSweeper.colors[4, 0]));
            Five.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[5, 2], MineSweeper.colors[5, 1], MineSweeper.colors[5, 0]));
            Six.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[6, 2], MineSweeper.colors[6, 1], MineSweeper.colors[6, 0]));
            Flag.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[10, 2], MineSweeper.colors[10, 1], MineSweeper.colors[10, 0]));
            
            //shift keys
            if (MineSweeper.useBackground)
            {
                ShiftFlag.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[9, 2], MineSweeper.colors[9, 1], MineSweeper.colors[9, 0]));
                shift1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[9, 2], MineSweeper.colors[9, 1], MineSweeper.colors[9, 0]));
            }
            else
            {
                ShiftFlag.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[16, 2], MineSweeper.colors[16, 1], MineSweeper.colors[16, 0]));
                shift1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[16, 2], MineSweeper.colors[16, 1], MineSweeper.colors[16, 0]));
            }

            ShiftFlag.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[16, 2], MineSweeper.colors[16, 1], MineSweeper.colors[16, 0]));
            Bomb.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[7, 2], MineSweeper.colors[7, 1], MineSweeper.colors[7, 0]));
            Covered.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[8, 2], MineSweeper.colors[8, 1], MineSweeper.colors[8, 0]));
            New.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[11, 2], MineSweeper.colors[11, 1], MineSweeper.colors[11, 0]));
            Default.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[14, 2], MineSweeper.colors[14, 1], MineSweeper.colors[14, 0]));
            Victory.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[13, 2], MineSweeper.colors[13, 1], MineSweeper.colors[13, 0]));
            Defeat.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[12, 2], MineSweeper.colors[12, 1], MineSweeper.colors[12, 0]));

            MineSweeper.printLogiLED();
        }

        public void UpdateFile()
        {
            string[] lines = { "Wins: " + MineSweeper.Wins, "Bombs: " + MineSweeper.Bombs, "Layout: " + MineSweeper.KeyboardLayout, "Total: " + MineSweeper.Total.ToString(), "Losses: " + MineSweeper.Losses.ToString(), "UseBackground: " + MineSweeper.useBackground };
            File.WriteAllLines(Config.fileConfig, lines);
        }

        public void UpdateStats()
        {
            var file = Config.fileStatistics[MineSweeper.KeyboardLayout];

            gloWins.Content = MineSweeper.Wins.ToString();
            gloLosses.Content = MineSweeper.Losses.ToString();
            gloTotal.Content = MineSweeper.Total.ToString();
            locTotal.Content = File.ReadLines(file).Skip(MineSweeper.Bombs + 63).Take(1).First().ToString();
            locLosses.Content = File.ReadLines(file).Skip(MineSweeper.Bombs + 42).Take(1).First().ToString();
            local.Content = "Statistics for " + Config.textForLayout[MineSweeper.KeyboardLayout] + " with " + MineSweeper.Bombs.ToString() + " Bombs:";
            locWins.Content = File.ReadLines(file).Skip(MineSweeper.Bombs + 21).Take(1).First().ToString();
            locBest.Content = BestTime(MineSweeper.Bombs);

            UpdateFile();
        }

        #endregion

        #region button difficulties
        
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = Config.easy.ToString();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = Config.medium.ToString();
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = Config.hard.ToString();
        }

        #endregion

        #region reset

        private void Button_set(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset settings", "Are you sure you want to reset? All settings will be lost.",
            MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetSettings();

                MineSweeper.newGame();

                StopWatchDefeat();
                ResetWatch();
            }
        }

        private void Button_col(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset colors", "Are you sure you want to reset? All colors will be lost.",
            MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetColors();
            }
        }

        private void Button_sta(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset statistics", "Are you sure you want to reset? All statistics will be lost.",
            MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetStatistics();
                
                StopWatchDefeat();
                ResetWatch();

                MineSweeper.newGame();
            }
        }

        private void Button_all(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset all settings and statistics", "Are you sure you want to reset?",
               MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetStatistics();
                ResetColors();
                ResetSettings();

                MineSweeper.newGame();

                StopWatchDefeat();
                ResetWatch();
            }
        }

        private void ResetColors()
        { 
            //writing the file
            File.WriteAllLines(Config.fileColors, Config.colorsDefault);

            //then reading the values out of it so you dont have to have seperate color array in config
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

            UpdateColors();
        }

        private void ResetSettings()
        {
            string[] lines = { "Wins: " + MineSweeper.Wins, "Bombs: " + Config.bombsDefault, "Layout: " + Config.keyboardLayoutDefault, "Total: 0", "Losses: 0", "UseBackground: " + Config.useBackgroundDefault };

            File.WriteAllLines(Config.fileConfig, Config.configDefault);

            MineSweeper.useBackground = Config.useBackgroundDefault; ;
            MineSweeper.Bombs = Config.bombsDefault;
            MineSweeper.KeyboardLayout = Config.keyboardLayoutDefault;
            KeyLayout.SelectedIndex = Config.keyboardLayoutDefault;
            NUDTextBox.Text = Config.bombsDefault.ToString();

            UpdateStats();
        }

        private void ResetStatistics()
        {
            string[] lines = { "Wins: 0", "Bombs: " + MineSweeper.Bombs, "Layout: " + MineSweeper.KeyboardLayout, "Total: 0", "Losses: 0", "UseBackground: " + MineSweeper.useBackground };

            File.WriteAllLines(Config.fileConfig, lines);

            foreach (string file in Config.fileStatistics)
            {
                File.WriteAllLines(file, Config.statisticsDefault);
            }

            timer1.Content = "30:00";
            MineSweeper.Wins = 0;
            MineSweeper.Losses = 0;
            MineSweeper.Total = 0;

            UpdateStats();
        }

        #endregion

        #region Color Tab Item

        //popup creator for less code
        private void ColorPopupCreator(int index, string text, System.Windows.Shapes.Rectangle rect1, System.Windows.Shapes.Rectangle rect2)
        {
            //saving color before so it can later be set again if cancel is pressed
            byte[] current = { MineSweeper.colors[index, 0], MineSweeper.colors[index, 1], MineSweeper.colors[index, 2] };

            if ((ColorPopup.Show(text, System.Windows.Media.Color.FromArgb(0xFF, MineSweeper.colors[index, 2], MineSweeper.colors[index, 1], MineSweeper.colors[index, 0]), MessageBoxButton.OKCancel, index, rect1) == MessageBoxResult.OK))
            {
                string[] colors = new string[Config.colorsDefault.Length];
                for (int i = 0; i < MineSweeper.colors.GetLength(0); i++)
                {
                    colors[i] = File.ReadLines(Config.fileColors).Skip(i).Take(1).First();
                }
                colors[index] = MineSweeper.colors[index, 0].ToString().PadLeft(3, '0') + "," + MineSweeper.colors[index, 1].ToString().PadLeft(3, '0') + "," + MineSweeper.colors[index, 2].ToString().PadLeft(3, '0');
                File.WriteAllLines(Config.fileColors, colors);
            }
            else
            {
                MineSweeper.colors[index, 2] = current[2];
                MineSweeper.colors[index, 1] = current[1];
                MineSweeper.colors[index, 0] = current[0];
                UpdateColors();
            }
            rect1.Fill = a;
            rect2.Stroke = new SolidColorBrush(Colors.Black);
            rect2.StrokeThickness = 1;
        }
        
        //seperate function for gamefieldkeys so the switch function doesnt have to be in every single function
        private void ColorPopupCreator(int index, System.Windows.Shapes.Rectangle rect1, System.Windows.Shapes.Rectangle rect2)
        {
            string text = "";
            switch (index)
            {
                case 0:
                    text = "0 Bombs";
                    break;
                case 1:
                    text = "1 Bomb";
                    break;
                case 2:
                    text = "2 Bombs";
                    break;
                case 3:
                    text = "3 Bombs";
                    break;
                case 4:
                    text = "4 Bombs";
                    break;
                case 5:
                    text = "5 Bombs";
                    break;
                case 6:
                    text = "6 Bombs";
                    break;
                case 7:
                    text = "Bomb Field";
                    break;
                case 8:
                    text = "Covered Field";
                    break;
                case 9:
                    switch (MineSweeper.currentBack)
                    {
                        case 0:
                            index = 14;
                            text = "Default Background";
                            break;
                        case 1:
                            index = 13;
                            text = "Victory Background";
                            break;
                        case 2:
                            index = 12;
                            text = "Defeat Background";
                            break;
                    }
                    break;
                case 10:
                    text = "Flag";
                    break;
            }

            ColorPopupCreator(index, text, rect1, rect2);
        }

        // for the color picker list
        private void ColorPopupCreator(int index, string text)
        {
            byte[] current = { MineSweeper.colors[index, 0], MineSweeper.colors[index, 1], MineSweeper.colors[index, 2] };

            if ((ColorPopup.Show(text, System.Windows.Media.Color.FromArgb(0xFF, MineSweeper.colors[index, 2], MineSweeper.colors[index, 1], MineSweeper.colors[index, 0]), MessageBoxButton.OKCancel, index) == MessageBoxResult.OK))
            {
                string[] colors = new string[Config.colorsDefault.Length];

                for (int i = 0; i < MineSweeper.colors.GetLength(0); i++)
                {
                    colors[i] = File.ReadLines(Config.fileColors).Skip(i).Take(1).First();
                }
                colors[index] = MineSweeper.colors[index, 0].ToString().PadLeft(3, '0') + "," + MineSweeper.colors[index, 1].ToString().PadLeft(3, '0') + "," + MineSweeper.colors[index, 2].ToString().PadLeft(3, '0');
                File.WriteAllLines(Config.fileColors, colors);
            }
            else
            {
                MineSweeper.colors[index, 2] = current[2];
                MineSweeper.colors[index, 1] = current[1];
                MineSweeper.colors[index, 0] = current[0];
                UpdateColors();
            }
        }

        #region offboard
        //ESC
        private void escmouseup(object sender, RoutedEventArgs e)
        {
            int index = 8;
            string text = "Default Background";

            switch (MineSweeper.currentBack)
            {
                case 0:
                    index = 14;
                    text = "Default Background";
                    break;
                case 1:
                    index = 13;
                    text = "Victory Background";
                    break;
                case 2:
                    index = 12;
                    text = "Defeat Background";
                    break;
            }

            ColorPopupCreator(index, text, esc, esc1);
        }

        //actually enter
        private void escmousedown(object sender, RoutedEventArgs e)
        {
            esc.Fill = esc1.Fill;
            toFill = esc;
            fromFill = esc1;
            hovering = true;
        }

        private void escmouseleave(object sender, RoutedEventArgs e)
        {
            esc.Fill = a;
            hovering = false;
        }

        #endregion
        
        #region Shift
        //ESC
        private void shiftmouseup(object sender, RoutedEventArgs e)
        {
            int index = 16;
            string text = "Shift Keys";

            ColorPopupCreator(index, text, shift, shift1);
        }

        //actually enter
        private void shiftmousedown(object sender, RoutedEventArgs e)
        {
            shift.Fill = shift1.Fill;
            toFill = shift;
            fromFill = shift1;
            hovering = true;
        }

        private void shiftmouseleave(object sender, RoutedEventArgs e)
        {
            shift.Fill = a;
            hovering = false;
        }

        #endregion

        #region bomb counter

        //FUNCTION
        private void f1mouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(15, "Bomb Counter", f1, d1);
        }

        //actually leave
        private void f1mousedown(object sender, RoutedEventArgs e)
        {
            SolidColorBrush brush = d1.Fill as SolidColorBrush;
            if ((brush.Color.R + brush.Color.G + brush.Color.B) < 260)
            {
                FunctionForeground.Foreground = new SolidColorBrush(System.Windows.Media.Colors.White);
            }
            f1.Fill = d1.Fill;
        }

        private void f1mouseleave(object sender, RoutedEventArgs e)
        {
            FunctionForeground.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Black);
            f1.Fill = a;
        }

        #endregion

        #region gamefield event handler

        private void MouseEnterGameKeys(System.Windows.Shapes.Rectangle back, System.Windows.Shapes.Rectangle front)
        {
            SolidColorBrush brush = back.Fill as SolidColorBrush;
            if ((brush.Color.R + brush.Color.G + brush.Color.B) < 260)
            {
                Label[] t = ((Canvas)LogicalTreeHelper.GetParent(front)).Children.OfType<Label>().ToArray();

                foreach (Label a in t)
                {
                    a.Foreground = new SolidColorBrush(System.Windows.Media.Colors.White);
                }
            }

            front.Fill = back.Fill;
            back.Stroke = new SolidColorBrush(Colors.Red);
            back.StrokeThickness = 2;
            hovering = true;
            toFill = front;
            fromFill = back;
        }

        private void MouseLeaveGameKeys(System.Windows.Shapes.Rectangle back, System.Windows.Shapes.Rectangle front)
        {
            Label[] t = ((Canvas)LogicalTreeHelper.GetParent(front)).Children.OfType<Label>().ToArray();
            foreach (Label a in t)
            {
                a.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Black);
            }

            front.Fill = a;
            back.Stroke = new SolidColorBrush(Colors.Black);
            back.StrokeThickness = 1;
            hovering = false;
        }

        //event handlers for each button press

        //index is found by checking the game map and then using the found number to see what is currently printed
        private void g1mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[1, 1], g1, h1); }
        private void g1mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h1, g1); }
        private void g1mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h1, g1); }
        private void g2mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[2, 1], g2, h2); }
        private void g2mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h2, g2); }
        private void g2mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h2, g2); }
        private void g3mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[3, 1], g3, h3); }
        private void g3mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h3, g3); }
        private void g3mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h3, g3); }
        private void g4mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[4, 1], g4, h4); }
        private void g4mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h4, g4); }
        private void g4mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h4, g4); }
        private void g5mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[5, 1], g5, h5); }
        private void g5mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h5, g5); }
        private void g5mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h5, g5); }
        private void g6mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[6, 1], g6, h6); }
        private void g6mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h6, g6); }
        private void g6mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h6, g6); }
        private void g7mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[7, 1], g7, h7); }
        private void g7mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h7, g7); }
        private void g7mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h7, g7); }
        private void g8mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[8, 1], g8, h8); }
        private void g8mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h8, g8); }
        private void g8mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h8, g8); }
        private void g9mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[9, 1], g9, h9); }
        private void g9mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h9, g9); }
        private void g9mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h9, g9); }
        private void g10mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[10, 1], g10, h10); }
        private void g10mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h10, g10); }
        private void g10mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h10, g10); }
        private void g11mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[11, 1], g11, h11); }
        private void g11mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h11, g11); }
        private void g11mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h11, g11); }
        private void g12newmouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[12, 1], g12new, h12new); }
        private void g12newmousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h12new, g12new); }
        private void g12newmouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h12new, g12new); }
        private void g12mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[1, 2], g12, h12); }
        private void g12mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h12, g12); }
        private void g12mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h12, g12); }
        private void g13mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[2, 2], g13, h13); }
        private void g13mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h13, g13); }
        private void g13mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h13, g13); }
        private void g14mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[3, 2], g14, h14); }
        private void g14mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h14, g14); }
        private void g14mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h14, g14); }
        private void g15mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[4, 2], g15, h15); }
        private void g15mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h15, g15); }
        private void g15mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h15, g15); }
        private void g16mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[5, 2], g16, h16); }
        private void g16mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h16, g16); }
        private void g16mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h16, g16); }
        private void g17mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[6, 2], g17, h17); }
        private void g17mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h17, g17); }
        private void g17mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h17, g17); }
        private void g18mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[7, 2], g18, h18); }
        private void g18mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h18, g18); }
        private void g18mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h18, g18); }
        private void g19mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[8, 2], g19, h19); }
        private void g19mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h19, g19); }
        private void g19mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h19, g19); }
        private void g20mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[9, 2], g20, h20); }
        private void g20mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h20, g20); }
        private void g20mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h20, g20); }
        private void g21mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[10, 2], g21, h21); }
        private void g21mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h21, g21); }
        private void g21mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h21, g21); }
        private void g22mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[11, 2], g22, h22); }
        private void g22mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h22, g22); }
        private void g22mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h22, g22); }
        private void g23newmouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[12, 2], g23new, h23new); }
        private void g23newmousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h23new, g23new); }
        private void g23newmouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h23new, g23new); }
        private void g23mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[1, 3], g23, h23); }
        private void g23mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h23, g23); }
        private void g23mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h23, g23); }
        private void g24mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[2, 3], g24, h24); }
        private void g24mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h24, g24); }
        private void g24mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h24, g24); }
        private void g25mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[3, 3], g25, h25); }
        private void g25mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h25, g25); }
        private void g25mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h25, g25); }
        private void g26mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[4, 3], g26, h26); }
        private void g26mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h26, g26); }
        private void g26mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h26, g26); }
        private void g27mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[5, 3], g27, h27); }
        private void g27mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h27, g27); }
        private void g27mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h27, g27); }
        private void g28mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[6, 3], g28, h28); }
        private void g28mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h28, g28); }
        private void g28mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h28, g28); }
        private void g29mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[7, 3], g29, h29); }
        private void g29mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h29, g29); }
        private void g29mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h29, g29); }
        private void g30mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[8, 3], g30, h30); }
        private void g30mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h30, g30); }
        private void g30mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h30, g30); }
        private void g31mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[9, 3], g31, h31); }
        private void g31mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h31, g31); }
        private void g31mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h31, g31); }
        private void g32mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[10, 3], g32, h32); }
        private void g32mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h32, g32); }
        private void g32mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h32, g32); }
        private void g33mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[11, 3], g33, h33); }
        private void g33mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h33, g33); }
        private void g33mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h33, g33); }
        private void g34newmouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[12, 3], g34new, h34new); }
        private void g34newmousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h34new, g34new); }
        private void g34newmouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h34new, g34new); }
        private void g34mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[1, 4], g34, h34); }
        private void g34mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h34, g34); }
        private void g34mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h34, g34); }
        private void g35mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[2, 4], g35, h35); }
        private void g35mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h35, g35); }
        private void g35mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h35, g35); }
        private void g36mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[3, 4], g36, h36); }
        private void g36mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h36, g36); }
        private void g36mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h36, g36); }
        private void g37mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[4, 4], g37, h37); }
        private void g37mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h37, g37); }
        private void g37mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h37, g37); }
        private void g38mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[5, 4], g38, h38); }
        private void g38mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h38, g38); }
        private void g38mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h38, g38); }
        private void g39mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[6, 4], g39, h39); }
        private void g39mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h39, g39); }
        private void g39mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h39, g39); }
        private void g40mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[7, 4], g40, h40); }
        private void g40mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h40, g40); }
        private void g40mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h40, g40); }
        private void g41mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[8, 4], g41, h41); }
        private void g41mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h41, g41); }
        private void g41mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h41, g41); }
        private void g42mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[9, 4], g42, h42); }
        private void g42mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h42, g42); }
        private void g42mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h42, g42); }
        private void g43mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[10, 4], g43, h43); }
        private void g43mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h43, g43); }
        private void g43mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h43, g43); }
        private void g44mouseup(object sender, RoutedEventArgs e) { ColorPopupCreator(MineSweeper.display[11, 4], g44, h44); }
        private void g44mousedown(object sender, RoutedEventArgs e) { MouseEnterGameKeys(h44, g44); }
        private void g44mouseleave(object sender, RoutedEventArgs e) { MouseLeaveGameKeys(h44, g44); }


        #endregion

        #region Key and newgame

        private void zeromouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(0, "0 Bombs", zero, zero1);
        }

        //actually enter
        private void zeromousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(zero1, zero);
        }

        private void zeromouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(zero1, zero);
        }

        //ONE
        private void onemouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(1, "1 Bomb", one, one1);
        }

        //actually enter
        private void onemousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(one1, one);
        }

        private void onemouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(one1, one);
        }

        //TWO
        private void twomouseup(object sender, RoutedEventArgs e)
        { 
            ColorPopupCreator(2, "2 Bombs", two, two1);
        }

        //actually enter
        private void twomousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(two1, two);
        }

        private void twomouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(two1, two);
        }
        
        //three
        private void threemouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(3, "3 Bombs", three, three1);
        }

        //actually enter
        private void threemousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(three1, three);
        }

        private void threemouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(three1, three);
        }

        //four
        private void fourmouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(4, "4 Bombs", four, four1);
        }

        //actually enter
        private void fourmousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(four1, four);
        }

        private void fourmouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(four1, four);
        }

        //five
        private void fivemouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(5, "5 Bombs", five, five1);
        }

        //actually enter
        private void fivemousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(five1, five);
        }

        private void fivemouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(five1, five);
        }

        //six
        private void sixmouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(6, "6 Bombs", six, six1);
        }

        //actually enter
        private void sixmousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(six1, six);
        }

        private void sixmouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(six1, six);
        }

        //new
        private void newmouseup(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(11, "New Game Key", newGame, newGame1);
        }

        //actually enter
        private void newmousedown(object sender, RoutedEventArgs e)
        {
            MouseEnterGameKeys(newGame1, newGame);
        }

        private void newmouseleave(object sender, RoutedEventArgs e)
        {
            MouseLeaveGameKeys(newGame1, newGame);
        }

        #endregion

        #region Color Picker List

        private void One_Click(object sender, RoutedEventArgs e)
        {
            int index = 1;
            string text = "1 Bomb";
            ColorPopupCreator(index, text);
        }

        private void Two_Click(object sender, RoutedEventArgs e)
        {
            int index = 2;
            string text = "2 Bombs";
            ColorPopupCreator(index, text);
        }

        private void Three_Click(object sender, RoutedEventArgs e)
        {
            int index = 3;
            string text = "3 Bombs";
            ColorPopupCreator(index, text);
        }

        private void Four_Click(object sender, RoutedEventArgs e)
        {
            int index = 4;
            string text = "4 Bombs";
            ColorPopupCreator(index, text);
        }

        private void Five_Click(object sender, RoutedEventArgs e)
        {
            int index = 5;
            string text = "5 Bombs";
            ColorPopupCreator(index, text);
        }

        private void Six_Click(object sender, RoutedEventArgs e)
        {
            int index = 6;
            string text = "6 Bombs";
            ColorPopupCreator(index, text);
        }

        private void Zero_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            string text = "0 Bombs";
            ColorPopupCreator(index, text);
        }

        private void Flag_Click(object sender, RoutedEventArgs e)
        {
            int index = 10;
            string text = "Flag";
            ColorPopupCreator(index, text);
        }

        private void Bomb_Click(object sender, RoutedEventArgs e)
        {
            int index = 7;
            string text = "Bomb Field";
            ColorPopupCreator(index, text);
        }

        private void Covered_Click(object sender, RoutedEventArgs e)
        {
            int index = 8;
            string text = "Covered Field";
            ColorPopupCreator(index, text);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            int index = 11;
            string text = "New Game Key";
            ColorPopupCreator(index, text);
        }

        private void Default_Click(object sender, RoutedEventArgs e)
        {
            int index = 14;
            string text = "Default Background";
            ColorPopupCreator(index, text);
        }

        private void Victory_Click(object sender, RoutedEventArgs e)
        {
            int index = 13;
            string text = "Victory Background";
            ColorPopupCreator(index, text);
        }

        private void Defeat_Click(object sender, RoutedEventArgs e)
        {
            int index = 12;
            string text = "Defeat Background";
            ColorPopupCreator(index, text);
        }
        
        private void ShiftFlag_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)FlagUseBackground.IsChecked)
            {
                int index = 16;
                string text = "Shift Keys";
                ColorPopupCreator(index, text);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 0.4;
            MineSweeper.useBackground = true;
            UpdateColors();
            ShiftL.Visibility = Visibility.Hidden;
            UpdateFile();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 1;
            MineSweeper.useBackground = false;
            UpdateColors();
            ShiftL.Visibility = Visibility.Visible;
            UpdateFile();
        }

        #endregion

        #endregion
    }
}