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
    #region Blur
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
        #region Events

        public delegate void UpdateDisplayEventHandler();
        public delegate void ResetColorsEventHandler();
        public delegate void InitKeyboardUserControlsEventHandler(KeyboardLayoutChangedEventArgs index);

        //fires when switching to color trab item updates inapp keyboard display
        public static event UpdateDisplayEventHandler UpdateDisplayEvent;
        //fires when resetting the color updates all style sheets
        public static event ResetColorsEventHandler ResetColorsEvent;
        public static event InitKeyboardUserControlsEventHandler InitKeyboardUserControlsEvent;

        #endregion

        #region Constructor and Variables

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public bool KeyboardDisplayShown { get; set; } = false;

        //variable for minesweeper object
        public MineSweeper MineSweeper { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Stop();

            SaveFileSettings settings = new SaveFileSettings(Config.PathSettingsFile, Config.UseBackgroundDefault, Config.KeyboardLayoutDefaultIndex, Config.BombsDefault, Config.defaultSetLogiLogo, Config.MinBombs, Config.MaxBombs, Config.KeyboardLayouts.Length-1);
            MineSweeper = new MineSweeper(settings, new SaveFileGlobalStatistics(Config.PathGlobalStatisticsFile), Config.KeyboardLayouts[settings.LayoutIndex], new SaveFileColors(Config.PathColorsFile, Config.ColorsDefault));
            
            MineSweeper.StatsChangedEvent += new MineSweeper.UpdateStatsEventHandler(UpdateStats);
            MineSweeper.UpdateTimerEvent += new MineSweeper.TimerEventHandler(UpdateTimer);

            //add all keyboardlayouts to Combo Box
            foreach (KeyboardLayout layout in Config.KeyboardLayouts)
            {
                KeyLayout.Items.Add(layout.Text);
            }

            _menuTabControl.SelectedIndex = 0;
            
            Config.InitKeyboardLayoutsArray();
            //init the keyboardusercontrols and let the right one subscribe to the print board events
            InitKeyboardUserControlsEvent?.Invoke(new KeyboardLayoutChangedEventArgs(MineSweeper.KeyboardLayout.Index));

            //select current keylayout
            KeyLayout.SelectedIndex = MineSweeper.KeyboardLayout.Index;
            NUDTextBox.Text = Convert.ToString(MineSweeper.Bombs);

            UpdateStats();
            UpdateTimerText();
        }

        #endregion

        #region Blur

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

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (MineSweeper.Stopwatch.Elapsed.TotalMilliseconds >= Config.MaxTimerValue)
            {
                MineSweeper.NewGame();
            }

            // Updating the Label which displays the current second
            UpdateTimerText();
        }

        public void UpdateTimer(UpdateTimerEventArgs TimerState)
        {
            switch (TimerState.State)
            {
                case (int)MineSweeper.TimerStateEnum.started:
                    TimerDisplay.Foreground = new SolidColorBrush(Config.Default);
                    dispatcherTimer.IsEnabled = true;
                    UpdateTimerText();
                    break;
                case (int)MineSweeper.TimerStateEnum.resetButNotStarted:
                    TimerDisplay.Foreground = new SolidColorBrush(Config.Default);
                    dispatcherTimer.IsEnabled = false;
                    UpdateTimerText();
                    break;
                case (int)MineSweeper.TimerStateEnum.stoppedDefeat:
                    TimerDisplay.Foreground = new SolidColorBrush(Config.Defeat);
                    dispatcherTimer.IsEnabled = false;
                    UpdateTimerText();
                    break;
                case (int)MineSweeper.TimerStateEnum.stoppedVictory:
                    TimerDisplay.Foreground = new SolidColorBrush(Config.Victory);
                    dispatcherTimer.IsEnabled = false;
                    UpdateTimerText();
                    break;
                case (int)MineSweeper.TimerStateEnum.stoppedNewRecord:
                    TimerDisplay.Foreground = new SolidColorBrush(Config.NewRecord);
                    dispatcherTimer.IsEnabled = false;
                    UpdateTimerText();
                    TimerDisplay.Content += Config.TextNewRecord;
                    break;
            }
        }

        private void UpdateTimerText()
        {
            TimerDisplay.Content = GetTimeString(MineSweeper.Stopwatch.Elapsed);
        }

        private string GetTimeString(TimeSpan elapsed)
        {
            if (elapsed.Hours == 0)
            {
                return string.Format("{0:00}:{1:00}", elapsed.Minutes, elapsed.Seconds);
            }
            else
            {
                return string.Format("{0:00}:{1:00}:{2:00}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
            }
        }

        #endregion

        #region Minimize, Close and Drag window

        private void Stack_mousedown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Click_Minimize(object sender, RoutedEventArgs e)
        {
            KeyboardDisplayShown = false;
            WindowState = WindowState.Minimized;
            ((App)System.Windows.Application.Current).nIcon.Visible = true;
        }

        private void Click_Close(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Settings Tab Item

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
            MineSweeper.Bombs = Convert.ToInt32(NUDTextBox.Text);
            UpdateStats();
        }

        #endregion

        #region Selected Keylayout Changed

        private void KeyLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MineSweeper.KeyboardLayout = Config.KeyboardLayouts[KeyLayout.SelectedIndex];
            UpdateStats();
        }

        #endregion

        #region SetLogiLogo Checkbox
        
        private void CheckBoxSetLogiLogo_Checked(object sender, RoutedEventArgs e)
        {
            MineSweeper.SetLogiLogo = true;
        }

        private void CheckBoxSetLogiLogo_Unchecked(object sender, RoutedEventArgs e)
        {
            MineSweeper.SetLogiLogo = false;
        }

        #endregion

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
                    CheckBoxSetLogiLogo.IsChecked = MineSweeper.SetLogiLogo;
                    KeyboardDisplayShown = false;
                    SetAllButtonsNormal();
                    settings.Style = styleClone;
                    break;
                case 1:
                    FlagUseBackground.IsChecked = MineSweeper.UseBackground;
                    KeyboardDisplayContainer.Children.Clear();
                    KeyboardDisplayContainer.Children.Add(MineSweeper.KeyboardLayout.KeyboardDisplayPage as UserControl);
                    UpdateDisplayEvent?.Invoke();
                    KeyboardDisplayShown = true;
                    SetAllButtonsNormal();
                    colors.Style = styleClone;
                    break;
                case 2:
                    KeyboardDisplayShown = false;
                    SetAllButtonsNormal();
                    stats.Style = styleClone;
                    break;
                case 3:
                    KeyboardDisplayShown = false;
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

        #region Button Difficulties

        private void Click_Easy(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = MineSweeper.KeyboardLayout.Easy.ToString();
        }

        private void Click_Medium(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = MineSweeper.KeyboardLayout.Medium.ToString();
        }

        private void Click_Hard(object sender, RoutedEventArgs e)
        {
            NUDTextBox.Text = MineSweeper.KeyboardLayout.Hard.ToString();
        }

        #endregion

        #region Update Statistics Display

        public void UpdateStats()
        {
            gloWins.Content = MineSweeper.Wins.ToString();
            gloLosses.Content = MineSweeper.Losses.ToString();
            gloTotal.Content = MineSweeper.Total.ToString();
            locTotal.Content = MineSweeper.KeyboardLayout.SaveFile.GetTotal(MineSweeper.Bombs);
            locLosses.Content = MineSweeper.KeyboardLayout.SaveFile.GetLosses(MineSweeper.Bombs);
            local.Content = "Statistics for " + MineSweeper.KeyboardLayout.Text + " with " + MineSweeper.Bombs + " Bombs:";
            locWins.Content = MineSweeper.KeyboardLayout.SaveFile.GetWins(MineSweeper.Bombs);
            locBest.Content = MillisecondsToHoursMinutes(MineSweeper.KeyboardLayout.SaveFile.GetBestTime(MineSweeper.Bombs));
        }

        #endregion

        #region Reset Buttons Handlers

        private void Button_set(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset settings", "Are you sure you want to reset? All settings will be lost.", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetSettings();

                MineSweeper.NewGame();
            }
        }

        private void Button_col(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset Colors", "Are you sure you want to reset? All Colors will be lost.", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetColors();
            }
        }

        private void Button_sta(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset statistics", "Are you sure you want to reset? All statistics will be lost.", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetStatistics();

                MineSweeper.NewGame();
            }
        }

        private void Button_all(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset all settings and statistics", "Are you sure you want to reset?", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetStatistics();
                ResetColors();
                ResetSettings();

                MineSweeper.NewGame();
            }
        }

        private void ResetColors()
        {
            MineSweeper.ColorsFile.ResetToDefault();
            MineSweeper.Colors = MineSweeper.ColorsFile.SavedColors;
            MineSweeper.PrintLogiLED();

            ResetColorsEvent();
        }

        private void ResetSettings()
        {
            MineSweeper.Settings.ResetToDefault();
            KeyLayout.SelectedIndex = MineSweeper.Settings.LayoutIndex;
            NUDTextBox.Text = MineSweeper.Bombs.ToString();

            UpdateStats();
        }

        private void ResetStatistics()
        {
            MineSweeper.GlobalStats.ResetToDefault();

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
            byte[] current = { MineSweeper.Colors[index, 0], MineSweeper.Colors[index, 1], MineSweeper.Colors[index, 2] };
            
            if ((ColorPopup.Show(System.Windows.Media.Color.FromArgb(0xFF, MineSweeper.Colors[index, 2], MineSweeper.Colors[index, 1], MineSweeper.Colors[index, 0]), index) == MessageBoxResult.OK))
            {
                MineSweeper.ColorsFile.SavedColors = MineSweeper.Colors;
            }
            else
            {
                MineSweeper.Colors[index, 2] = current[2];
                MineSweeper.Colors[index, 1] = current[1];
                MineSweeper.Colors[index, 0] = current[0];

                Application.Current.Resources["buttonColorBrush" + index.ToString()] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.Colors[index, 2], MineSweeper.Colors[index, 1], MineSweeper.Colors[index, 0]));

                if (Config.ForegroundColorImportant.Contains(index))
                {
                    if (MineSweeper.Colors[index, 2] + MineSweeper.Colors[index, 1] + MineSweeper.Colors[index, 0] < Config.ForegroundThreshold)
                    {
                        Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.Black);
                    }
                }

                MineSweeper.PrintLogiLED();
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
            MineSweeper.UseBackground = true;
            MineSweeper.PrintLogiLED();
            ResetColorsEvent?.Invoke();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 1;
            MineSweeper.UseBackground = false;
            MineSweeper.PrintLogiLED();
            ResetColorsEvent?.Invoke();
        }

        #endregion

        #endregion
    }
}