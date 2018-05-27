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

        //default brush of the keyboard button foreground so you dont need to type it out
        public static SolidColorBrush a = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFC, 0xFC, 0xFC));

        //for changing color of hovereffect when key color changes in game
        public bool hovering = false;
        public System.Windows.Shapes.Rectangle toFill;
        public System.Windows.Shapes.Rectangle fromFill;

        public MainWindow()
        {
            InitializeComponent();

            _menuTabControl.SelectedIndex = 0;
            MineSweeper.main = App.Current.MainWindow as MainWindow;

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

            MineSweeper.newGame();
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

        string BestTime(int bombs)
        {
            string best = "";
            int a = 0;

            var file = Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile;

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

        private void DispatcherTimer_Tick(object sender, EventArgs e)
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
                var file = Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile;

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

        private void image1_MouseEnter(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/symbols/closeWhite.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void image1_MouseLeave(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/symbols/close.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

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

            UpdateFile();
            UpdateStats();
        }

        #endregion

        #region Nav-Buttons
        
        //if you use num keys to change selection
        private void TabSelectionChanged(object sender, RoutedEventArgs e)
        {
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

            FlagUseBackground.IsChecked = MineSweeper.useBackground;

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

        #region Update statistics and config files

        public void UpdateFile()
        {
            string[] lines = { "Wins: " + MineSweeper.Wins, "Bombs: " + MineSweeper.Bombs, "Layout: " + MineSweeper.KeyboardLayout, "Total: " + MineSweeper.Total.ToString(), "Losses: " + MineSweeper.Losses.ToString(), "UseBackground: " + MineSweeper.useBackground };
            File.WriteAllLines(Config.fileConfig, lines);
        }

        public void UpdateStats()
        {
            var file = Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile;

            gloWins.Content = MineSweeper.Wins.ToString();
            gloLosses.Content = MineSweeper.Losses.ToString();
            gloTotal.Content = MineSweeper.Total.ToString();
            locTotal.Content = File.ReadLines(file).Skip(MineSweeper.Bombs + 63).Take(1).First().ToString();
            locLosses.Content = File.ReadLines(file).Skip(MineSweeper.Bombs + 42).Take(1).First().ToString();
            local.Content = "Statistics for " + Config.KeyboardLayouts[MineSweeper.KeyboardLayout].Text + " with " + MineSweeper.Bombs.ToString() + " Bombs:";
            locWins.Content = File.ReadLines(file).Skip(MineSweeper.Bombs + 21).Take(1).First().ToString();
            locBest.Content = BestTime(MineSweeper.Bombs);

            UpdateFile();
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

            MineSweeper.printLogiLED();

            MineSweeper.useBackground = Config.useBackgroundDefault;

            ResetColorsEvent();

            UpdateFile();
        }

        private void ResetSettings()
        {
            string[] lines = { "Wins: " + MineSweeper.Wins, "Bombs: " + Config.bombsDefault, "Layout: " + Config.keyboardLayoutDefault, "Total: 0", "Losses: 0", "UseBackground: " + Config.useBackgroundDefault };

            File.WriteAllLines(Config.fileConfig, Config.configDefault);

            MineSweeper.useBackground = Config.useBackgroundDefault;
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

            foreach(KeyboardLayout layout in Config.KeyboardLayouts)
            {
                File.WriteAllLines(layout.SaveFile, Config.statisticsDefault);
            }

            timer1.Content = "30:00";
            MineSweeper.Wins = 0;
            MineSweeper.Losses = 0;
            MineSweeper.Total = 0;

            UpdateStats();
        }

        #endregion

        #region Color Picker List

        // for the color picker list
        private void ColorPopupCreator(int index)
        {
            byte[] current = { MineSweeper.colors[index, 0], MineSweeper.colors[index, 1], MineSweeper.colors[index, 2] };

            if ((ColorPopup.Show(Config.colorPickerTitles[index], System.Windows.Media.Color.FromArgb(0xFF, MineSweeper.colors[index, 2], MineSweeper.colors[index, 1], MineSweeper.colors[index, 0]), MessageBoxButton.OKCancel, index, true) == MessageBoxResult.OK))
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

                Application.Current.Resources["buttonColorBrush" + index.ToString()] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[index, 2], MineSweeper.colors[index, 1], MineSweeper.colors[index, 0]));

                if (index == 12 || index == 13 || index == 14)
                {
                    MineSweeper.colors[9, 0] = current[0];
                    MineSweeper.colors[9, 1] = current[1];
                    MineSweeper.colors[9, 2] = current[2];
                    Application.Current.Resources["buttonColorBrush9"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[9, 2], MineSweeper.colors[9, 1], MineSweeper.colors[9, 0]));
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
            MineSweeper.useBackground = true;
            MineSweeper.printLogiLED();
            UpdateFile();
            ResetColorsEvent();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShiftFlag.Opacity = 1;
            MineSweeper.useBackground = false;
            MineSweeper.printLogiLED();
            UpdateFile();
            ResetColorsEvent();
        }

        #endregion

        #endregion
    }
}