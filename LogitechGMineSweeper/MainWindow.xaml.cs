using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;

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

        public static DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private static readonly Stopwatch timer = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

            _menuTabControl.SelectedIndex = 0;

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Stop();
            
            UpdateTimer();

            NUDTextBox.Text = Convert.ToString(Config.MineSweeper.Bombs);

            UpdateStats();

            //add all keyboardlayouts to Combo Box
            foreach (KeyboardLayout layout in Config.KeyboardLayouts)
            {
                KeyLayout.Items.Add(layout.Text);
            }

            Config.InitKeyboardLayoutsArray();

            //select current keylayout
            KeyLayout.SelectedIndex = Config.MineSweeper.KeyboardLayout.Index;

            timer1.Foreground = new SolidColorBrush(Config.Default);

            SaveFileStatitics.PrintStatsEvent += new SaveFileStatitics.PrintStatsEventHandler(UpdateStats);
            MineSweeper.ResetWatchEvent += new MineSweeper.ResetWatchEventHandler(ResetWatch);
            MineSweeper.StopWatchDefeatEvent += new MineSweeper.StopWatchDefeatEventHandler(StopWatchDefeat);
            MineSweeper.StopWatchVictoryEvent += new MineSweeper.StopWatchVictoryEventHandler(StopWatchVictory);
            MineSweeper.StartWatchEvent += new MineSweeper.StartWatchEventHandler(StartWatch);
            MineSweeper.UpdateStatsEvent += new MineSweeper.UpdateStatsEventHandler(UpdateStats);
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
                return Config.TimeNotSetText;
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
            dispatcherTimer.IsEnabled = false;
            UpdateTimer();

            int bestTime = Config.MineSweeper.KeyboardLayout.SaveFile.GetBestTime(Config.MineSweeper.Bombs);

            if (bestTime == -1 || bestTime > timer.Elapsed.TotalMilliseconds)
            {
                timer1.Foreground = new SolidColorBrush(Config.NewRecord);
                timer1.Content += Config.TextNewRecord;

                Config.MineSweeper.KeyboardLayout.SaveFile.UpdateBestTime(Config.MineSweeper.Bombs, Convert.ToInt32(timer.Elapsed.TotalMilliseconds));
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (timer.Elapsed.TotalMilliseconds >= Config.MaxTimerValue)
            {
                Config.MineSweeper.NewGame();
                StopWatchDefeat();
                ResetWatch();
            }

            // Updating the Label which displays the current second
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            timer1.Content = GetTimeString(timer.Elapsed);
        }

        public void StopWatchDefeat()
        {
            timer1.Foreground = new SolidColorBrush(Config.Defeat);
            timer.Stop();
            dispatcherTimer.IsEnabled = false;
        }

        public void StartWatch()
        {
            timer1.Foreground = new SolidColorBrush(Config.Default);
            timer.Reset();
            dispatcherTimer.IsEnabled = true;
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
            MineSweeper.KeyboardDisplayShown = false;
            WindowState = WindowState.Minimized;
            ((App)System.Windows.Application.Current).nIcon.Visible = true;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Numeric Up-Down - Bomb Setting

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number < Config.MaxBombs)
                NUDTextBox.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number > Config.MinBombs)
                NUDTextBox.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox.Text != "")
                if (!int.TryParse(NUDTextBox.Text, out number)) NUDTextBox.Text = Config.NumUDstartvalue.ToString();
            if (number > Config.MaxBombs) NUDTextBox.Text = Config.MaxBombs.ToString();
            if (number < Config.MinBombs) NUDTextBox.Text = Config.MinBombs.ToString();
            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;
            Config.MineSweeper.Bombs = Convert.ToInt32(NUDTextBox.Text);
            Config.MineSweeper.NewGame();

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
                    MineSweeper.KeyboardDisplayShown = false;
                    SetAllButtonsNormal();
                    settings.Style = styleClone;
                    break;
                case 1:
                    FlagUseBackground.IsChecked = Config.MineSweeper.UseBackground;
                    KeyboardDisplayContainer.Children.Clear();
                    KeyboardDisplayContainer.Children.Add(Config.MineSweeper.KeyboardLayout.KeyboardDisplayPage as UserControl);
                    UpdateDisplayEvent();
                    MineSweeper.KeyboardDisplayShown = true;
                    SetAllButtonsNormal();
                    colors.Style = styleClone;
                    break;
                case 2:
                    MineSweeper.KeyboardDisplayShown = false;
                    SetAllButtonsNormal();
                    stats.Style = styleClone;
                    break;
                case 3:
                    MineSweeper.KeyboardDisplayShown = false;
                    SetAllButtonsNormal();
                    reset.Style = styleClone;
                    break;
            }
        }

        private void Click_Settings(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 0;
        }

        private void Click_Colors(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 1;
        }

        private void Click_Statistics(object sender, RoutedEventArgs e)
        {
            _menuTabControl.SelectedIndex = 2;
        }

        private void Click_Reset(object sender, RoutedEventArgs e)
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

        #endregion

        #region selected keylayout changed

        private void KeyLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.MineSweeper.KeyboardLayout = Config.KeyboardLayouts[KeyLayout.SelectedIndex];

            Config.MineSweeper.NewGame();

            StopWatchDefeat();
            ResetWatch();
            UpdateStats();
        }

        #endregion

        #region button difficulties

        private void Click_Easy(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = Config.MineSweeper.KeyboardLayout.Easy.ToString();
        }

        private void Click_Medium(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = Config.MineSweeper.KeyboardLayout.Medium.ToString();
        }

        private void Click_Hard(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = Config.MineSweeper.KeyboardLayout.Hard.ToString();
        }

        #endregion

        #region Update statistics display

        public void UpdateStats()
        {
            gloWins.Content = Config.MineSweeper.Wins.ToString();
            gloLosses.Content = Config.MineSweeper.Losses.ToString();
            gloTotal.Content = Config.MineSweeper.Total.ToString();
            locTotal.Content = Config.MineSweeper.KeyboardLayout.SaveFile.GetTotal(Config.MineSweeper.Bombs);
            locLosses.Content = Config.MineSweeper.KeyboardLayout.SaveFile.GetLosses(Config.MineSweeper.Bombs);
            local.Content = "Statistics for " + Config.MineSweeper.KeyboardLayout.Text + " with " + Config.MineSweeper.Bombs + " Bombs:";
            locWins.Content = Config.MineSweeper.KeyboardLayout.SaveFile.GetWins(Config.MineSweeper.Bombs);
            locBest.Content = MillisecondsToHoursMinutes(Config.MineSweeper.KeyboardLayout.SaveFile.GetBestTime(Config.MineSweeper.Bombs));
        }

        #endregion

        #region reset

        private void Button_set(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset settings", "Are you sure you want to reset? All settings will be lost.",
            MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetSettings();

                Config.MineSweeper.NewGame();

                StopWatchDefeat();
                ResetWatch();
            }
        }

        private void Button_col(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset Colors", "Are you sure you want to reset? All Colors will be lost.",
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

                Config.MineSweeper.NewGame();
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

                Config.MineSweeper.NewGame();

                StopWatchDefeat();
                ResetWatch();
            }
        }

        private void ResetColors()
        {
            Config.MineSweeper.ColorsFile.ResetToDefault();
            Config.MineSweeper.Colors = Config.MineSweeper.ColorsFile.SavedColors;
            Config.MineSweeper.PrintLogiLED();

            ResetColorsEvent();
        }

        private void ResetSettings()
        {
            Config.MineSweeper.Settings.ResetToDefault();
            KeyLayout.SelectedIndex = Config.KeyboardLayoutDefaultIndex;
            NUDTextBox.Text = Config.MineSweeper.Bombs.ToString();

            UpdateStats();
        }

        private void ResetStatistics()
        {
            Config.MineSweeper.GlobalStats.ResetToDefault();

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
            byte[] current = { Config.MineSweeper.Colors[index, 0], Config.MineSweeper.Colors[index, 1], Config.MineSweeper.Colors[index, 2] };

            if ((ColorPopup.Show(System.Windows.Media.Color.FromArgb(0xFF, Config.MineSweeper.Colors[index, 2], Config.MineSweeper.Colors[index, 1], Config.MineSweeper.Colors[index, 0]), index) == MessageBoxResult.OK))
            {
                Config.MineSweeper.ColorsFile.SavedColors = Config.MineSweeper.Colors;
            }
            else
            {
                Config.MineSweeper.Colors[index, 2] = current[2];
                Config.MineSweeper.Colors[index, 1] = current[1];
                Config.MineSweeper.Colors[index, 0] = current[0];

                Application.Current.Resources["buttonColorBrush" + index.ToString()] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(Config.MineSweeper.Colors[index, 2], Config.MineSweeper.Colors[index, 1], Config.MineSweeper.Colors[index, 0]));

                if (Config.ForegroundColorImportant.Contains(index))
                {
                    if (Config.MineSweeper.Colors[index, 2] + Config.MineSweeper.Colors[index, 1] + Config.MineSweeper.Colors[index, 0] < Config.ForegroundThreshold)
                    {
                        Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.Black);
                    }
                }

                Config.MineSweeper.PrintLogiLED();
            }
        }

        #region Color Picker List

        private void One_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Sourrounding1);
        }

        private void Two_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Sourrounding2);
        }

        private void Three_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Sourrounding3);
        }

        private void Four_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Sourrounding4);
        }

        private void Five_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Sourrounding5);
        }

        private void Six_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Sourrounding6);
        }

        private void Zero_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Sourrounding0);
        }

        private void Flag_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Flag);
        }

        private void Bomb_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Mine);
        }

        private void Covered_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Covered);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.NewGame);
        }

        private void Default_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundDefault);
        }

        private void Victory_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundVictory);
        }

        private void Defeat_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundDefeat);
        }
        
        private void ShiftFlag_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)FlagUseBackground.IsChecked)
            {
                ColorPopupCreator((int)MineSweeper.MapEnum.Shift);
            }
        }

        private void Counter_Click(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Counter);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 0.4;
            Config.MineSweeper.UseBackground = true;
            Config.MineSweeper.PrintLogiLED();
            ResetColorsEvent();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 1;
            Config.MineSweeper.UseBackground = false;
            Config.MineSweeper.PrintLogiLED();
            ResetColorsEvent();
        }

        #endregion

        #endregion
    }
}