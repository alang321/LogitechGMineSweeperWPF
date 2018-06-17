using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogitechGMineSweeper.KeyboardLayouts
{
    /// <summary>
    /// Interaction logic for DE.xaml
    /// </summary>
    public partial class DE : UserControl
    {
        #region Constructor and variables

        Button[] board;

        Button[] key;

        Button[] function;

        Style[] styles;

        int activeAtLayout = (int)Config.Layout.DE;

        public DE()
        {
            InitializeComponent();

            MineSweeper.PrintEvent += new MineSweeper.PrintdisplayEventHandler(PrintEvent);

            MainWindow.UpdateDisplayEvent += new MainWindow.UpdateDisplayEventHandler(UpdateDisplayEvent);

            MainWindow.ResetColorsEvent += new MainWindow.ResetColorsEventHandler(ResetColorsEvent);

            board = new Button[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11,
                                   p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23,
                                   p24, p25, p26, p27, p28, p29, p30, p31, p32, p33, p34, p35,
                                   p36, p37, p38, p39, p40, p41, p42, p43, p44, p45, p46, p47 };

            key = new Button[] { k0, k1, k2, k3, k4, k5, k6 };

            function = new Button[] { f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11 };

            Style style0 = Application.Current.FindResource("buttonColor0") as Style;
            Style style1 = Application.Current.FindResource("buttonColor1") as Style;
            Style style2 = Application.Current.FindResource("buttonColor2") as Style;
            Style style3 = Application.Current.FindResource("buttonColor3") as Style;
            Style style4 = Application.Current.FindResource("buttonColor4") as Style;
            Style style5 = Application.Current.FindResource("buttonColor5") as Style;
            Style style6 = Application.Current.FindResource("buttonColor6") as Style;
            Style style7 = Application.Current.FindResource("buttonColor7") as Style;
            Style style8 = Application.Current.FindResource("buttonColor8") as Style;
            Style style9 = Application.Current.FindResource("buttonColor9") as Style;
            Style style10 = Application.Current.FindResource("buttonColor10") as Style;
            Style style12 = Application.Current.FindResource("buttonColor12") as Style;
            Style style13 = Application.Current.FindResource("buttonColor13") as Style;
            Style style14 = Application.Current.FindResource("buttonColor14") as Style;
            Style style15 = Application.Current.FindResource("buttonColor15") as Style;
            Style enterStyle12 = Application.Current.FindResource("Enter12") as Style;
            Style enterStyle13 = Application.Current.FindResource("Enter13") as Style;
            Style enterStyle14 = Application.Current.FindResource("Enter14") as Style;
            styles = new Style[] { style0, style1, style2, style3, style4, style5, style6, style7, style8, style9, style10, null, style12, style13, style14, style15, enterStyle12, enterStyle13, enterStyle14 };

            InitDisplay();
        }

        #endregion

        #region Initialization

        //subscribe to select color tab event
        private void InitDisplay()
        {
            Application.Current.Resources["buttonColorBrush0"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[0, 2], Config.MineSweeper.Colors[0, 1], Config.MineSweeper.Colors[0, 0]));
            Application.Current.Resources["buttonColorBrush1"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[1, 2], Config.MineSweeper.Colors[1, 1], Config.MineSweeper.Colors[1, 0]));
            Application.Current.Resources["buttonColorBrush2"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[2, 2], Config.MineSweeper.Colors[2, 1], Config.MineSweeper.Colors[2, 0]));
            Application.Current.Resources["buttonColorBrush3"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[3, 2], Config.MineSweeper.Colors[3, 1], Config.MineSweeper.Colors[3, 0]));
            Application.Current.Resources["buttonColorBrush4"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[4, 2], Config.MineSweeper.Colors[4, 1], Config.MineSweeper.Colors[4, 0]));
            Application.Current.Resources["buttonColorBrush5"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[5, 2], Config.MineSweeper.Colors[5, 1], Config.MineSweeper.Colors[5, 0]));
            Application.Current.Resources["buttonColorBrush6"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[6, 2], Config.MineSweeper.Colors[6, 1], Config.MineSweeper.Colors[6, 0]));
            Application.Current.Resources["buttonColorBrush7"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[7, 2], Config.MineSweeper.Colors[7, 1], Config.MineSweeper.Colors[7, 0]));
            Application.Current.Resources["buttonColorBrush8"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[8, 2], Config.MineSweeper.Colors[8, 1], Config.MineSweeper.Colors[8, 0]));
            Application.Current.Resources["buttonColorBrush9"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[9, 2], Config.MineSweeper.Colors[9, 1], Config.MineSweeper.Colors[9, 0]));
            Application.Current.Resources["buttonColorBrush10"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[10, 2], Config.MineSweeper.Colors[10, 1], Config.MineSweeper.Colors[10, 0]));
            Application.Current.Resources["buttonColorBrush11"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[11, 2], Config.MineSweeper.Colors[11, 1], Config.MineSweeper.Colors[11, 0]));
            Application.Current.Resources["buttonColorBrush12"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[12, 2], Config.MineSweeper.Colors[12, 1], Config.MineSweeper.Colors[12, 0]));
            Application.Current.Resources["buttonColorBrush13"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[13, 2], Config.MineSweeper.Colors[13, 1], Config.MineSweeper.Colors[13, 0]));
            Application.Current.Resources["buttonColorBrush14"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[14, 2], Config.MineSweeper.Colors[14, 1], Config.MineSweeper.Colors[14, 0]));
            Application.Current.Resources["buttonColorBrush15"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[15, 2], Config.MineSweeper.Colors[15, 1], Config.MineSweeper.Colors[15, 0]));
            Application.Current.Resources["buttonColorBrush16"] = new SolidColorBrush(Color.FromRgb(Config.MineSweeper.Colors[16, 2], Config.MineSweeper.Colors[16, 1], Config.MineSweeper.Colors[16, 0]));

            //foreground brushes
            for(int index = 0; index < Config.MineSweeper.Colors.Length; index++)
            {
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
                else
                {
                    Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.Transparent);
                }
            }

            if (Config.MineSweeper.UseBackground)
            {
                ShiftL.Visibility = Visibility.Hidden;
            }
            else
            {
                ShiftL.Visibility = Visibility.Visible;
            }

            PrintBoard();
        }

        #endregion

        #region Print Event Handlers

        private void PrintEvent()
        {
            if (Config.MineSweeper.KeyboardLayout.Index == activeAtLayout)
            {
                PrintBoard();
            }
        }

        private void UpdateDisplayEvent()
        {
            if (Config.MineSweeper.KeyboardLayout.Index == activeAtLayout)
            {
                PrintBoard();

                if (Config.MineSweeper.UseBackground)
                {
                    ShiftL.Visibility = Visibility.Hidden;
                }
                else
                {
                    ShiftL.Visibility = Visibility.Visible;
                }
            }
        }

        private void ResetColorsEvent()
        {
            if (Config.MineSweeper.KeyboardLayout.Index == activeAtLayout)
            {
                InitDisplay();
            }
        }

        #endregion

        #region color popups

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

        private void ClickNoFunc(object sender, RoutedEventArgs e)
        {
            switch (Config.MineSweeper.GameState)
            {
                case MineSweeper.GameStateEnum.Default:
                    ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundDefault);
                    break;
                case MineSweeper.GameStateEnum.Victory:
                    ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundVictory);
                    break;
                case MineSweeper.GameStateEnum.Defeat:
                    ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundDefeat);
                    break;
            }
        }

        private void ClickGameField(object sender, RoutedEventArgs e)
        {
            Button pressed = sender as Button;
            int i = Array.IndexOf(board, pressed);
            int index = Config.MineSweeper.Display[(i % 12 + 1), (i / 12 + 1)];

            if (index == 9)
            {
                switch (Config.MineSweeper.GameState)
                {
                    case MineSweeper.GameStateEnum.Default:
                        ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundDefault);
                        break;
                    case MineSweeper.GameStateEnum.Victory:
                        ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundVictory);
                        break;
                    case MineSweeper.GameStateEnum.Defeat:
                        ColorPopupCreator((int)MineSweeper.MapEnum.BackgroundDefeat);
                        break;
                }
            }

            ColorPopupCreator(index);
        }

        private void ClickNewGame(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.NewGame);
        }

        private void ClickKey(object sender, RoutedEventArgs e)
        {
            Button pressed = sender as Button;
            ColorPopupCreator(Array.IndexOf(key, pressed));
        }

        private void ClickCount(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Counter);
        }

        private void ClickFlagMod(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator((int)MineSweeper.MapEnum.Shift);
        }

        #endregion

        #region print board

        private void PrintBoard()
        {
            switch (Config.MineSweeper.GameState)
            {
                case MineSweeper.GameStateEnum.Default:
                    esc.Style = styles[(int)MineSweeper.MapEnum.BackgroundDefault];
                    enter.Style = styles[(int)MineSweeper.MapEnum.BackgroundDefault + 4];
                    break;
                case MineSweeper.GameStateEnum.Victory:
                    esc.Style = styles[(int)MineSweeper.MapEnum.BackgroundVictory];
                    enter.Style = styles[(int)MineSweeper.MapEnum.BackgroundVictory + 4];
                    break;
                case MineSweeper.GameStateEnum.Defeat:
                    esc.Style = styles[(int)MineSweeper.MapEnum.BackgroundDefeat];
                    enter.Style = styles[(int)MineSweeper.MapEnum.BackgroundDefeat + 4];
                    break;
            }

            //for actually printing the board
            int counter = 0;
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 12; j++)
                {
                    if (Config.MineSweeper.Display[j, i] == (int)MineSweeper.MapEnum.BackgroundPlaceholder)
                    {
                        switch (Config.MineSweeper.GameState)
                        {
                            case MineSweeper.GameStateEnum.Default:
                                board[counter++].Style = styles[(int)MineSweeper.MapEnum.BackgroundDefault];
                                break;
                            case MineSweeper.GameStateEnum.Victory:
                                board[counter++].Style = styles[(int)MineSweeper.MapEnum.BackgroundVictory];
                                break;
                            case MineSweeper.GameStateEnum.Defeat:
                                board[counter++].Style = styles[(int)MineSweeper.MapEnum.BackgroundDefeat];
                                break;
                        }
                    }
                    else
                    {
                        board[counter++].Style = styles[Config.MineSweeper.Display[j, i]];
                    }
                }
            }

            for (int i = 0; i < 12; i++)
            {
                if(Config.MineSweeper.Display[i + 1, 0] == (int)MineSweeper.MapEnum.Counter) function[i].Visibility = Visibility.Visible;
                else function[i].Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}
