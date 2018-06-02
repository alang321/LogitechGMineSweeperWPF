using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using System.Windows.Navigation;

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
        
        public delegate void UpdateDisplayEventHandler();
        public delegate void ResetColorsEventHandler();

        //fires when switching to color trab item updates inapp eyboard display
        public static event UpdateDisplayEventHandler UpdateDisplayEvent;
        //fires when resetting the color updates all style sheets
        public static event ResetColorsEventHandler ResetColorsEvent;

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public static System.Timers.Timer dispatcherTimer = new System.Timers.Timer();
        private static readonly Stopwatch timer = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

            _menuTabControl.SelectedIndex = 0;
            MineSweeper.Main = App.Current.MainWindow as MainWindow;

            dispatcherTimer.Elapsed += new ElapsedEventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = 1000;
            dispatcherTimer.Stop();
            
            UpdateTimer();

            NUDTextBox.Text = Convert.ToString(MineSweeper.Bombs);

            UpdateStats();

            //add all keyboardlayouts to Combo Box
            foreach (KeyboardLayout layout in Config.KeyboardLayouts)
            {
                KeyLayout.Items.Add(layout.Text);
            }

            Config.InitKeyboardLayoutsArray();

            //select current keylayout
            KeyLayout.SelectedIndex = MineSweeper.KeyboardLayout;

            timer1.Foreground = new SolidColorBrush(Config.Default);

            SaveFileStatitics.PrintStatsEvent += new SaveFileStatitics.PrintStatsEventHandler(PrintStatsEvent);
        }

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

        string MillisecondsToHoursMinutes(int ms)
        {
            if (ms == -1)
            {
                return Config.timeNotSet;
            }

            TimeSpan t = TimeSpan.FromMilliseconds(ms);

            if(t.Hours == 0)
            {
                return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            }
            else
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
            }

        }

        public void StopWatchVictory()
        {
            timer1.Foreground = new SolidColorBrush(Config.Victory);
            timer.Stop();
            dispatcherTimer.Enabled = false;
            UpdateTimer();

            int bestTime = Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.GetBestTime(MineSweeper.Bombs);

            if (bestTime == -1 || bestTime > timer.Elapsed.TotalMilliseconds)
            {
                timer1.Foreground = new SolidColorBrush(Config.NewRecord);
                timer1.Content += Config.textNewRecord;

                Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.UpdateBestTime(MineSweeper.Bombs, Convert.ToInt32(timer.Elapsed.TotalMilliseconds));
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (timer.Elapsed.TotalMilliseconds >= Config.maxTimerValue)
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

        private void UpdateTimer()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                timer1.Content = GetTimeString(timer.Elapsed);
            });
        }

        public void StopWatchDefeat()
        {
            timer1.Foreground = new SolidColorBrush(Config.Defeat);
            timer.Stop();
            dispatcherTimer.Enabled = false;
        }

        public void StartWatch()
        {
            timer1.Foreground = new SolidColorBrush(Config.Default);
            timer.Reset();
            dispatcherTimer.Enabled = true;
            timer.Start();
        }

        public void ResetWatch()
        {
            timer1.Foreground = new SolidColorBrush(Config.Default);
            timer.Reset();
            UpdateTimer();
        }

        private string GetTimeString(TimeSpan elapsed)
        {
            if (elapsed.Hours == 0)
            {
                return string.Format("{0:00}:{1:00}", elapsed.Minutes, elapsed.Seconds);
            }
            else
            {
                return string.Format("{0:00}:{1:00}:{2:00} - Record!", elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
            }
        }

        #endregion

        #region minimize, close and drag window

        private void Stack_mousedown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
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

            UpdateStats();
        }

        #endregion

        #region Nav-Buttons
        
        //if you use num keys to change selection
        private void TabSelectionChanged(object sender, RoutedEventArgs e)
        {
            var style = new Style();

            style.Setters.Add(new Setter(TemplateProperty, this.FindResource("navButtonActive")));
            
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

            switch (_menuTabControl.SelectedIndex)
            {
                case 0:
                    SetAllButtonsNormal();
                    settings.Style = styleClone;
                    break;
                case 1:
                    SetAllButtonsNormal();
                    colors.Style = styleClone;
                    break;
                case 2:
                    SetAllButtonsNormal();
                    stats.Style = styleClone;
                    break;
                case 3:
                    SetAllButtonsNormal();
                    reset.Style = styleClone;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 1;

            FlagUseBackground.IsChecked = MineSweeper.UseBackground;

            KeyboardDisplayContainer.Children.Clear();

            KeyboardDisplayContainer.Children.Add(Config.KeyboardLayouts[MineSweeper.KeyboardLayout].KeyboardDisplayPage as UserControl);

            UpdateDisplayEvent();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 2;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 3;
        }

        private void SetAllButtonsNormal()
        {
            var style = new Style();

            style.Setters.Add(new Setter(TemplateProperty, this.FindResource("navButton")));

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
            Config.fileConfig.Layout = MineSweeper.KeyboardLayout;
            MineSweeper.newGame();

            StopWatchDefeat();
            ResetWatch();
            UpdateStats();
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

        #region Update statistics

        public void UpdateStats()
        {
            gloWins.Content = Config.fileConfig.Wins.ToString();
            gloLosses.Content = Config.fileConfig.Losses.ToString();
            gloTotal.Content = Config.fileConfig.Total.ToString();
            locTotal.Content = Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.GetTotal(MineSweeper.Bombs);
            locLosses.Content = Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.GetLosses(MineSweeper.Bombs);
            local.Content = "Statistics for " + Config.KeyboardLayouts[MineSweeper.KeyboardLayout].Text + " with " + MineSweeper.Bombs.ToString() + " Bombs:";
            locWins.Content = Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.GetWins(MineSweeper.Bombs);
            locBest.Content = MillisecondsToHoursMinutes(Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.GetBestTime(MineSweeper.Bombs));
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
            Config.fileColors.ResetToDefault(ref MineSweeper.colors);

            MineSweeper.printLogiLED();

            MineSweeper.UseBackground = Config.useBackgroundDefault;
            Config.fileConfig.UseBackground = MineSweeper.UseBackground;

            ResetColorsEvent();
        }

        private void ResetSettings()
        {
            Config.fileConfig.Bombs = Config.bombsDefault;
            Config.fileConfig.Layout = Config.keyboardLayoutDefault;
            Config.fileConfig.UseBackground = Config.useBackgroundDefault;
            MineSweeper.UseBackground = Config.useBackgroundDefault;
            MineSweeper.Bombs = Config.bombsDefault;
            MineSweeper.KeyboardLayout = Config.keyboardLayoutDefault;

            KeyLayout.SelectedIndex = Config.keyboardLayoutDefault;
            NUDTextBox.Text = Config.bombsDefault.ToString();

            UpdateStats();
        }

        private void ResetStatistics()
        {
            Config.fileConfig.Wins = 0;
            Config.fileConfig.Total = 0;
            Config.fileConfig.Losses = 0;

            foreach(KeyboardLayout layout in Config.KeyboardLayouts)
            {
                layout.SaveFile.ResetToDefault();
            }

            UpdateStats();
        }

        #endregion

        #region Color Picker List

        // for the color picker list
        private void ColorPopupCreator(int index)
        {
            byte[] current = { MineSweeper.colors[index, 0], MineSweeper.colors[index, 1], MineSweeper.colors[index, 2] };

            if ((ColorPopup.Show(System.Windows.Media.Color.FromArgb(0xFF, MineSweeper.colors[index, 2], MineSweeper.colors[index, 1], MineSweeper.colors[index, 0]), index) == MessageBoxResult.OK))
            {
                Config.fileColors.SavedColors = MineSweeper.colors;
            }
            else
            {
                MineSweeper.colors[index, 2] = current[2];
                MineSweeper.colors[index, 1] = current[1];
                MineSweeper.colors[index, 0] = current[0];

                Application.Current.Resources["buttonColorBrush" + index.ToString()] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[index, 2], MineSweeper.colors[index, 1], MineSweeper.colors[index, 0]));

                if (Config.foregroundColorImportant.Contains(index))
                {
                    if (MineSweeper.colors[index, 2] + MineSweeper.colors[index, 1] + MineSweeper.colors[index, 0] < Config.foregroundThreshold)
                    {
                        Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.Black);
                    }
                }

                MineSweeper.printLogiLED();
            }
        }

        #region Color Picker List

        private void One_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(1);
        }

        private void Two_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(2);
        }

        private void Three_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(3);
        }

        private void Four_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(4);
        }

        private void Five_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(5);
        }

        private void Six_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(6);
        }

        private void Zero_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(0);
        }

        private void Flag_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(10);
        }

        private void Bomb_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(7);
        }

        private void Covered_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(8);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(11);
        }

        private void Default_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(14);
        }

        private void Victory_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(13);
        }

        private void Defeat_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(12);
        }
        
        private void ShiftFlag_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)FlagUseBackground.IsChecked)
            {
                ColorPopupCreator(16);
            }
        }

        private void Counter_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(15);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 0.4;
            MineSweeper.UseBackground = true;
            Config.fileConfig.UseBackground = MineSweeper.UseBackground;
            MineSweeper.printLogiLED();
            ResetColorsEvent();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 1;
            MineSweeper.UseBackground = false;
            Config.fileConfig.UseBackground = MineSweeper.UseBackground;
            MineSweeper.printLogiLED();
            ResetColorsEvent();
        }

        #endregion

        #endregion

        #region misc

        public void PrintStatsEvent()
        {
            UpdateStats();
        }

        #endregion
    }
}