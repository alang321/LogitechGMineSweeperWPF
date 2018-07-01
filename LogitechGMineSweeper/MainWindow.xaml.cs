using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Media.Animation;

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

        enum PagesStartingPoints { settings = 0, colors = -405, stats = -810, reset = -1215 }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public bool KeyboardDisplayShown { get; set; } = false;

        //variable for minesweeper object
        public MineSweeper MineSweeper { get; set; }

        public int SelectedIndex { get; set; } = 0;

        #region objects animation

        //HamburgerMenu
        bool Collapsed = true;

        DoubleAnimation pointAnimationHamburgerMenu = new DoubleAnimation()
        {
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5,
            Duration = new Duration(TimeSpan.FromSeconds(0.3))
        };

        ThicknessAnimation thicknessCollapseAnimationHamburgerMenu = new ThicknessAnimation()
        {
            To = new Thickness(7, 0, 0, 0),
            BeginTime = TimeSpan.FromSeconds(0.25),
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5,
            Duration = new Duration(TimeSpan.FromSeconds(0.1))
        };

        ThicknessAnimation thicknessExpandAnimationHamburgerMenu = new ThicknessAnimation()
        {
            To = new Thickness(0, 0, 0, 0),
            BeginTime = TimeSpan.FromSeconds(0),
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5,
            Duration = new Duration(TimeSpan.FromSeconds(0.1))
        };

        ThicknessAnimation thicknessAnimationTimer = new ThicknessAnimation()
        {
            BeginTime = TimeSpan.FromSeconds(0),
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5,
            Duration = new Duration(TimeSpan.FromSeconds(0.3))
        };

        /*ColorAnimation colorAnimationHamburgerMenu = new ColorAnimation()
        {
            BeginTime = TimeSpan.FromSeconds(0),
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5,
            Duration = new Duration(TimeSpan.FromSeconds(0.3))
        };*/

        #region Animation arrow

        Storyboard HamburgerArrow = new Storyboard();
        Storyboard HamburgerNormal = new Storyboard();
        //arrow
        //topline

        DoubleAnimation arrowTopLineAnimationHamburgerMenuY1 = new DoubleAnimation()
        {
            AccelerationRatio = Config.ArrowAcceleration,
            DecelerationRatio = Config.ArrowDeceleration,
            Duration = Config.ArrowAnimDuration
        };

        DoubleAnimation arrowTopLineAnimationHamburgerMenuY2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.ArrowAcceleration,
            DecelerationRatio = Config.ArrowDeceleration,
            Duration = Config.ArrowAnimDuration
        };

        DoubleAnimation arrowTopLineAnimationHamburgerMenuX2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.ArrowAcceleration,
            DecelerationRatio = Config.ArrowDeceleration,
            Duration = Config.ArrowAnimDuration
        };

        //bottomline

        DoubleAnimation arrowBottomLineAnimationHamburgerMenuY1 = new DoubleAnimation()
        {
            AccelerationRatio = Config.ArrowAcceleration,
            DecelerationRatio = Config.ArrowDeceleration,
            Duration = Config.ArrowAnimDuration
        };

        DoubleAnimation arrowBottomLineAnimationHamburgerMenuY2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.ArrowAcceleration,
            DecelerationRatio = Config.ArrowDeceleration,
            Duration = Config.ArrowAnimDuration
        };

        DoubleAnimation arrowBottomLineAnimationHamburgerMenuX2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.ArrowAcceleration,
            DecelerationRatio = Config.ArrowDeceleration,
            Duration = Config.ArrowAnimDuration
        };

        //hamburger
        //topline

        DoubleAnimation hamburgerTopLineAnimationHamburgerMenuY1 = new DoubleAnimation()
        {
            AccelerationRatio = Config.HamburgerAcceleration,
            DecelerationRatio = Config.HamburgerDeceleration,
            Duration = Config.HamburgerAnimDuration
        };

        DoubleAnimation hamburgerTopLineAnimationHamburgerMenuX2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.HamburgerAcceleration,
            DecelerationRatio = Config.HamburgerDeceleration,
            Duration = Config.HamburgerAnimDuration
        };

        DoubleAnimation hamburgerTopLineAnimationHamburgerMenuY2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.HamburgerAcceleration,
            DecelerationRatio = Config.HamburgerDeceleration,
            Duration = Config.HamburgerAnimDuration
        };

        //bottomline

        DoubleAnimation hamburgerBottomLineAnimationHamburgerMenuY1 = new DoubleAnimation()
        {
            AccelerationRatio = Config.HamburgerAcceleration,
            DecelerationRatio = Config.HamburgerDeceleration,
            Duration = Config.HamburgerAnimDuration
        };

        DoubleAnimation hamburgerBottomLineAnimationHamburgerMenuX2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.HamburgerAcceleration,
            DecelerationRatio = Config.HamburgerDeceleration,
            Duration = Config.HamburgerAnimDuration
        };

        DoubleAnimation hamburgerBottomLineAnimationHamburgerMenuY2 = new DoubleAnimation()
        {
            AccelerationRatio = Config.HamburgerAcceleration,
            DecelerationRatio = Config.HamburgerDeceleration,
            Duration = Config.HamburgerAnimDuration
        };

        #endregion

        //nav menu
        Button[] NavButtons;

        DoubleAnimation pointAnimationSelected = new DoubleAnimation()
        {
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5,
            Duration = new Duration(TimeSpan.FromSeconds(0.15))
        };

        DoubleAnimation pointAnimationNavSlide = new DoubleAnimation()
        {
            AccelerationRatio = 0.5,
            DecelerationRatio = 0.5,
            Duration = new Duration(TimeSpan.FromSeconds(0.3))
        };

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            
            AllInit();
        }

        #endregion

        #region Initialization

        private void AllInit()
        {
            InitMinesweeper();
            Config.InitKeyboardLayoutsArray();
            InitKeyboardLayoutDisplay();
            InitAnimations();
            InitTimer();
            InitUI();
            UpdateStats();
        }

        private void InitMinesweeper()
        {
            SaveFileSettings settingsFile = new SaveFileSettings(Config.PathSettingsFile, Config.UseBackgroundDefault, Config.KeyboardLayoutDefaultIndex, Config.BombsDefault, Config.defaultSetLogiLogo, Config.MinBombs, Config.MaxBombs, Config.KeyboardLayouts.Length - 1);
            MineSweeper = new MineSweeper(settingsFile, new SaveFileGlobalStatistics(Config.PathGlobalStatisticsFile), Config.KeyboardLayouts[settingsFile.LayoutIndex], new SaveFileColors(Config.PathColorsFile, Config.ColorsDefault));

            MineSweeper.StatsChangedEvent += new MineSweeper.UpdateStatsEventHandler(UpdateStats);
            MineSweeper.UpdateTimerEvent += new MineSweeper.TimerEventHandler(UpdateTimer);
        }

        private void InitKeyboardLayoutDisplay()
        {
            //init the keyboardusercontrols and let the right one subscribe to the print board events
            InitKeyboardUserControlsEvent?.Invoke(new KeyboardLayoutChangedEventArgs(MineSweeper.KeyboardLayout.Index));
            KeyboardDisplayContainer.Children.Add(MineSweeper.KeyboardLayout.KeyboardDisplayPage as UserControl);
            MineSweeper.KeyLayoutChangedEvent += LayoutChangedHandler;
        }

        private void InitAnimations()
        {
            //arrow

            //TopLine
            //Y1
            arrowTopLineAnimationHamburgerMenuY1.To = 10;
            Storyboard.SetTarget(arrowTopLineAnimationHamburgerMenuY1, LineTop);
            Storyboard.SetTargetProperty(arrowTopLineAnimationHamburgerMenuY1, new PropertyPath("(Line.Y1)"));
            HamburgerArrow.Children.Add(arrowTopLineAnimationHamburgerMenuY1);
            //X2
            arrowTopLineAnimationHamburgerMenuX2.To = 10;
            Storyboard.SetTarget(arrowTopLineAnimationHamburgerMenuX2, LineTop);
            Storyboard.SetTargetProperty(arrowTopLineAnimationHamburgerMenuX2, new PropertyPath("(Line.X2)"));
            HamburgerArrow.Children.Add(arrowTopLineAnimationHamburgerMenuX2);
            //Y2
            arrowTopLineAnimationHamburgerMenuY2.To = 3;
            Storyboard.SetTarget(arrowTopLineAnimationHamburgerMenuY2, LineTop);
            Storyboard.SetTargetProperty(arrowTopLineAnimationHamburgerMenuY2, new PropertyPath("(Line.Y2)"));
            HamburgerArrow.Children.Add(arrowTopLineAnimationHamburgerMenuY2);

            //BottomLine
            //Y1
            arrowBottomLineAnimationHamburgerMenuY1.To = 10;
            Storyboard.SetTarget(arrowBottomLineAnimationHamburgerMenuY1, LineBottom);
            Storyboard.SetTargetProperty(arrowBottomLineAnimationHamburgerMenuY1, new PropertyPath("(Line.Y1)"));
            HamburgerArrow.Children.Add(arrowBottomLineAnimationHamburgerMenuY1);
            //X2
            arrowBottomLineAnimationHamburgerMenuX2.To = 10;
            Storyboard.SetTarget(arrowBottomLineAnimationHamburgerMenuX2, LineBottom);
            Storyboard.SetTargetProperty(arrowBottomLineAnimationHamburgerMenuX2, new PropertyPath("(Line.X2)"));
            HamburgerArrow.Children.Add(arrowBottomLineAnimationHamburgerMenuX2);
            //Y2
            arrowBottomLineAnimationHamburgerMenuY2.To = 17;
            Storyboard.SetTarget(arrowBottomLineAnimationHamburgerMenuY2, LineBottom);
            Storyboard.SetTargetProperty(arrowBottomLineAnimationHamburgerMenuY2, new PropertyPath("(Line.Y2)"));
            HamburgerArrow.Children.Add(arrowBottomLineAnimationHamburgerMenuY2);

            //Hamburger

            //TopLine
            //Y1
            hamburgerTopLineAnimationHamburgerMenuY1.To = 5;
            Storyboard.SetTarget(hamburgerTopLineAnimationHamburgerMenuY1, LineTop);
            Storyboard.SetTargetProperty(hamburgerTopLineAnimationHamburgerMenuY1, new PropertyPath("(Line.Y1)"));
            HamburgerNormal.Children.Add(hamburgerTopLineAnimationHamburgerMenuY1);
            //X2
            hamburgerTopLineAnimationHamburgerMenuX2.To = 18;
            Storyboard.SetTarget(hamburgerTopLineAnimationHamburgerMenuX2, LineTop);
            Storyboard.SetTargetProperty(hamburgerTopLineAnimationHamburgerMenuX2, new PropertyPath("(Line.X2)"));
            HamburgerNormal.Children.Add(hamburgerTopLineAnimationHamburgerMenuX2);
            //Y2
            hamburgerTopLineAnimationHamburgerMenuY2.To = 5;
            Storyboard.SetTarget(hamburgerTopLineAnimationHamburgerMenuY2, LineTop);
            Storyboard.SetTargetProperty(hamburgerTopLineAnimationHamburgerMenuY2, new PropertyPath("(Line.Y2)"));
            HamburgerNormal.Children.Add(hamburgerTopLineAnimationHamburgerMenuY2);

            //BottomLine
            //Y1
            hamburgerBottomLineAnimationHamburgerMenuY1.To = 15;
            Storyboard.SetTarget(hamburgerBottomLineAnimationHamburgerMenuY1, LineBottom);
            Storyboard.SetTargetProperty(hamburgerBottomLineAnimationHamburgerMenuY1, new PropertyPath("(Line.Y1)"));
            HamburgerNormal.Children.Add(hamburgerBottomLineAnimationHamburgerMenuY1);
            //X2
            hamburgerBottomLineAnimationHamburgerMenuX2.To = 18;
            Storyboard.SetTarget(hamburgerBottomLineAnimationHamburgerMenuX2, LineBottom);
            Storyboard.SetTargetProperty(hamburgerBottomLineAnimationHamburgerMenuX2, new PropertyPath("(Line.X2)"));
            HamburgerNormal.Children.Add(hamburgerBottomLineAnimationHamburgerMenuX2);
            //Y2
            hamburgerBottomLineAnimationHamburgerMenuY2.To = 15;
            Storyboard.SetTarget(hamburgerBottomLineAnimationHamburgerMenuY2, LineBottom);
            Storyboard.SetTargetProperty(hamburgerBottomLineAnimationHamburgerMenuY2, new PropertyPath("(Line.Y2)"));
            HamburgerNormal.Children.Add(hamburgerBottomLineAnimationHamburgerMenuY2);

            //Side Bar
            NavButtons = new Button[] { settings, colors, stats, reset };
        }

        private void InitTimer()
        {
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Stop();
        }

        private void InitUI()
        {
            //add all keyboardlayouts to Combo Box
            foreach (KeyboardLayout layout in Config.KeyboardLayouts)
            {
                KeyLayout.Items.Add(layout.Text);
            }
            UpdateTimerText();
            //select current keylayout
            KeyLayout.SelectedIndex = MineSweeper.KeyboardLayout.Index;
            NUDTextBox.Text = Convert.ToString(MineSweeper.Bombs);
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

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        #endregion

        #region Hamburger Menu
        
        private void Click_Ham(object sender, RoutedEventArgs e)
        {
            if (Collapsed)
            {
                Expand();
            }
            else
            {
                Collapse();
            }
        }

        private void Click_Cover(object sender, RoutedEventArgs e)
        {
            Collapse();
        }

        private void Collapse()
        {
            Collapsed = true;
            pointAnimationHamburgerMenu.To = 45;
            Cover.IsHitTestVisible = false;
            Stack1.BeginAnimation(TextBlock.MarginProperty, thicknessCollapseAnimationHamburgerMenu);
            thicknessAnimationTimer.To = new Thickness(0, 165, 0, 0);
            TimerDisplay.BeginAnimation(Label.MarginProperty, thicknessAnimationTimer);
            settings.RenderTransformOrigin = new Point(0.108, 0.49);
            //colorAnimationHamburgerMenu.To = Color.FromArgb(0, 0, 0, 0);
            //CoverColor.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimationHamburgerMenu);
            HamMenu.BeginAnimation(DockPanel.WidthProperty, pointAnimationHamburgerMenu);
            HamburgerNormal.Begin();
        }

        private void Expand()
        {
            Collapsed = false;
            pointAnimationHamburgerMenu.To = 183;
            Cover.IsHitTestVisible = true;
            Stack1.BeginAnimation(TextBlock.MarginProperty, thicknessExpandAnimationHamburgerMenu);
            thicknessAnimationTimer.To = new Thickness(10, 165, 0, 0);
            TimerDisplay.BeginAnimation(Label.MarginProperty, thicknessAnimationTimer);
            settings.RenderTransformOrigin = new Point(0.5, 0.5);
            //colorAnimationHamburgerMenu.To = Color.FromArgb(100, 0, 0, 0);
            //CoverColor.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimationHamburgerMenu);
            HamMenu.BeginAnimation(DockPanel.WidthProperty, pointAnimationHamburgerMenu);
            HamburgerArrow.Begin();
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

            if (t.Hours == 0)
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
            if (MineSweeper.Stopwatch.Elapsed >= Config.MaxTimerValue)
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

        #region KeyboardLayoutChanged EventHandler

        private void LayoutChangedHandler(KeyboardLayoutChangedEventArgs index)
        {
            KeyboardDisplayContainer.Children.Clear();
            KeyboardDisplayContainer.Children.Add(MineSweeper.KeyboardLayout.KeyboardDisplayPage as UserControl);
        }

        #endregion

        #region Nav-Buttons

        private void TabSelectionChanged(int selectedIndex)
        {
            if (selectedIndex != this.SelectedIndex)
            {
                //if its gonna pass the color selector tab update display
                if ((selectedIndex > 1 && this.SelectedIndex == 0) || (this.SelectedIndex > 1 && selectedIndex == 0))
                {
                    UpdateDisplayEvent?.Invoke();
                    FlagUseBackground.IsChecked = MineSweeper.UseBackground;
                }

                this.SelectedIndex = selectedIndex;

                switch (selectedIndex)
                {
                    case 0:
                        CheckBoxSetLogiLogo.IsChecked = MineSweeper.SetLogiLogo;
                        KeyboardDisplayShown = false;
                        pointAnimationSelected.To = Canvas.GetTop(settings);
                        pointAnimationNavSlide.To = (int)PagesStartingPoints.settings;
                        AnimatedSideBarSelected.BeginAnimation(Canvas.TopProperty, pointAnimationSelected);
                        Pages.BeginAnimation(Canvas.TopProperty, pointAnimationNavSlide);
                        break;
                    case 1:
                        KeyboardDisplayShown = true;
                        UpdateDisplayEvent?.Invoke();
                        FlagUseBackground.IsChecked = MineSweeper.UseBackground;
                        pointAnimationSelected.To = Canvas.GetTop(colors);
                        pointAnimationNavSlide.To = (int)PagesStartingPoints.colors;
                        AnimatedSideBarSelected.BeginAnimation(Canvas.TopProperty, pointAnimationSelected);
                        Pages.BeginAnimation(Canvas.TopProperty, pointAnimationNavSlide);
                        break;
                    case 2:
                        KeyboardDisplayShown = false;
                        pointAnimationSelected.To = Canvas.GetTop(stats);
                        pointAnimationNavSlide.To = (int)PagesStartingPoints.stats;
                        AnimatedSideBarSelected.BeginAnimation(Canvas.TopProperty, pointAnimationSelected);
                        Pages.BeginAnimation(Canvas.TopProperty, pointAnimationNavSlide);
                        break;
                    case 3:
                        KeyboardDisplayShown = false;
                        pointAnimationSelected.To = Canvas.GetTop(reset);
                        pointAnimationNavSlide.To = (int)PagesStartingPoints.reset;
                        AnimatedSideBarSelected.BeginAnimation(Canvas.TopProperty, pointAnimationSelected);
                        Pages.BeginAnimation(Canvas.TopProperty, pointAnimationNavSlide);
                        break;
                }
            }
        }

        private void Click_Settings(object sender, RoutedEventArgs e)
        {
            TabSelectionChanged(0);
        }
        private void Click_Colors(object sender, RoutedEventArgs e)
        {
            TabSelectionChanged(1);
        }

        private void Click_Statistics(object sender, RoutedEventArgs e)
        {
            TabSelectionChanged(2);
        }

        private void Click_Reset(object sender, RoutedEventArgs e)
        {
            TabSelectionChanged(3);
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
            if ((MessageBox.Show("Reset settings", "Are you sure you want to reset all settings?", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetSettings();

                MineSweeper.NewGame();
            }
        }

        private void Button_col(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset colors", "Are you sure you want to reset all colors?", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                ResetColors();
            }
        }

        private void Button_sta(object sender, RoutedEventArgs e)
        {
            if ((MessageBox.Show("Reset statistics", "Are you sure you want to reset all statistics?", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
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

            foreach (KeyboardLayout layout in Config.KeyboardLayouts)
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