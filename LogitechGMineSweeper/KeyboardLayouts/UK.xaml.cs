using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogitechGMineSweeper.KeyboardLayouts
{
    /// <summary>
    /// Interaction logic for UK.xaml
    /// </summary>
    public partial class UK : UserControl
    {
        #region Constructor and variables

        Button[] board;
        
        Button[] key;

        Button[] function;

        Style[] styles;
        
        int activeAtLayout = (int)Config.Layout.UK;

        public UK()
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
            Style enterStyle12 = Application.Current.FindResource("Enter12") as Style;
            Style enterStyle13 = Application.Current.FindResource("Enter13") as Style;
            Style enterStyle14 = Application.Current.FindResource("Enter14") as Style;
            styles = new Style[] { style0, style1, style2, style3, style4, style5, style6, style7, style8, style9, style10, null, style12, style13, style14, enterStyle12, enterStyle13, enterStyle14 };

            InitDisplay();
        }

        #endregion

        #region Initialization

        //subscribe to select color tab event
        private void InitDisplay()
        {
            Application.Current.Resources["buttonColorBrush0"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[0, 2], MineSweeper.colors[0, 1], MineSweeper.colors[0, 0]));
            Application.Current.Resources["buttonColorBrush1"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[1, 2], MineSweeper.colors[1, 1], MineSweeper.colors[1, 0]));
            Application.Current.Resources["buttonColorBrush2"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[2, 2], MineSweeper.colors[2, 1], MineSweeper.colors[2, 0]));
            Application.Current.Resources["buttonColorBrush3"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[3, 2], MineSweeper.colors[3, 1], MineSweeper.colors[3, 0]));
            Application.Current.Resources["buttonColorBrush4"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[4, 2], MineSweeper.colors[4, 1], MineSweeper.colors[4, 0]));
            Application.Current.Resources["buttonColorBrush5"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[5, 2], MineSweeper.colors[5, 1], MineSweeper.colors[5, 0]));
            Application.Current.Resources["buttonColorBrush6"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[6, 2], MineSweeper.colors[6, 1], MineSweeper.colors[6, 0]));
            Application.Current.Resources["buttonColorBrush7"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[7, 2], MineSweeper.colors[7, 1], MineSweeper.colors[7, 0]));
            Application.Current.Resources["buttonColorBrush8"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[8, 2], MineSweeper.colors[8, 1], MineSweeper.colors[8, 0]));
            Application.Current.Resources["buttonColorBrush9"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[9, 2], MineSweeper.colors[9, 1], MineSweeper.colors[9, 0]));
            Application.Current.Resources["buttonColorBrush10"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[10, 2], MineSweeper.colors[10, 1], MineSweeper.colors[10, 0]));
            Application.Current.Resources["buttonColorBrush11"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[11, 2], MineSweeper.colors[11, 1], MineSweeper.colors[11, 0]));
            Application.Current.Resources["buttonColorBrush12"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[12, 2], MineSweeper.colors[12, 1], MineSweeper.colors[12, 0]));
            Application.Current.Resources["buttonColorBrush13"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[13, 2], MineSweeper.colors[13, 1], MineSweeper.colors[13, 0]));
            Application.Current.Resources["buttonColorBrush14"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[14, 2], MineSweeper.colors[14, 1], MineSweeper.colors[14, 0]));
            Application.Current.Resources["buttonColorBrush15"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[15, 2], MineSweeper.colors[15, 1], MineSweeper.colors[15, 0]));
            Application.Current.Resources["buttonColorBrush16"] = new SolidColorBrush(Color.FromRgb(MineSweeper.colors[16, 2], MineSweeper.colors[16, 1], MineSweeper.colors[16, 0]));

            //foreground brushes
            for (int index = 0; index < MineSweeper.colors.Length; index++)
            {
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
                else
                {
                    Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.Transparent);
                }
            }

            if (Config.fileConfig.UseBackground)
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
            if (MineSweeper.KeyboardLayout == activeAtLayout)
            {
                PrintBoard();
            }
        }

        private void UpdateDisplayEvent()
        {
            if (MineSweeper.KeyboardLayout == activeAtLayout)
            {
                PrintBoard();
            }
        }

        private void ResetColorsEvent()
        {
            if (MineSweeper.KeyboardLayout == activeAtLayout)
            {
                InitDisplay();
            }
        }

        #endregion

        #region color popups

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

        private void ClickNoFunc(object sender, RoutedEventArgs e)
        {
            switch (MineSweeper.GameState)
            {
                case (int)MineSweeper.GameStateEnum.Default:
                    ColorPopupCreator(14);
                    break;
                case (int)MineSweeper.GameStateEnum.Victory:
                    ColorPopupCreator(13);
                    break;
                case (int)MineSweeper.GameStateEnum.Defeat:
                    ColorPopupCreator(12);
                    break;
            }
        }

        private void ClickGameField(object sender, RoutedEventArgs e)
        {
            Button pressed = sender as Button;
            int i = Array.IndexOf(board, pressed);
            int index = MineSweeper.Display[(i % 12 + 1), (i / 12 + 1)];

            if (index == 9)
            {
                switch (MineSweeper.GameState)
                {
                    case (int)MineSweeper.GameStateEnum.Default:
                        index = 14;
                        break;
                    case (int)MineSweeper.GameStateEnum.Victory:
                        index = 13;
                        break;
                    case (int)MineSweeper.GameStateEnum.Defeat:
                        index = 12;
                        break;
                }
            }

            ColorPopupCreator(index);
        }

        private void ClickNewGame(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(11);
        }

        private void ClickKey(object sender, RoutedEventArgs e)
        {
            Button pressed = sender as Button;
            ColorPopupCreator(Array.IndexOf(key, pressed));
        }

        private void ClickCount(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(15);
        }

        private void ClickFlagMod(object sender, RoutedEventArgs e)
        {
            ColorPopupCreator(16);
        }

        #endregion

        #region print board

        private void PrintBoard()
        {
            switch (MineSweeper.GameState)
            {
                case (int)MineSweeper.GameStateEnum.Default:
                    esc.Style = styles[14];
                    enter.Style = styles[17];
                    break;
                case (int)MineSweeper.GameStateEnum.Victory:
                    esc.Style = styles[13];
                    enter.Style = styles[16];
                    break;
                case (int)MineSweeper.GameStateEnum.Defeat:
                    esc.Style = styles[12];
                    enter.Style = styles[15];
                    break;
            }

            //for actually printing the board
            int counter = 0;
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 12; j++)
                {
                    if (MineSweeper.Display[j, i] == 9)
                    {
                        switch (MineSweeper.GameState)
                        {
                            case (int)MineSweeper.GameStateEnum.Default:
                                board[counter++].Style = styles[14];
                                break;
                            case (int)MineSweeper.GameStateEnum.Victory:
                                board[counter++].Style = styles[13];
                                break;
                            case (int)MineSweeper.GameStateEnum.Defeat:
                                board[counter++].Style = styles[12];
                                break;
                        }
                    }
                    else
                    {
                        board[counter++].Style = styles[MineSweeper.Display[j, i]];
                    }
                }
            }

            if (MineSweeper.GameState == (int)MineSweeper.GameStateEnum.Default)
            {
                foreach (Button a in function)
                {
                    a.Visibility = Visibility.Collapsed;
                }

                for (int i = 0; i < MineSweeper.Bombs - MineSweeper.Flagged; i++)
                {
                    if (i >= 12) break;
                    function[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                foreach (Button a in function)
                {
                    a.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion
    }
}
