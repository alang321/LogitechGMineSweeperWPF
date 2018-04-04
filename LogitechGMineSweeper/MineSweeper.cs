using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace LogitechGMineSweeper
{
    static class MineSweeper
    {
        #region Variables Constructor and Properties

        static int[,] map;
        static bool[,] isBomb;
        static bool[,] isFlag = new bool[13, 6];
        public static int[,] display;
        static int bombs = 13;
        static int covered = 11 * 4;
        static int wins = 0;
        static int total = 0;
        static int losses = 0;
        static bool gameRunning;
        static bool firstMove = true;
        static string keyboardLayout = "US";
        static bool setBackground = false;
        public static int currentBack = 0;
        public static LogitechGMineSweeper.MainWindow main;

        static int flagged = 0;

        public static byte[,] colors =
            {
                // bombs sourrounding counter
                {000,000,000},  //0
                {000,127,255},  //1
                {255,255,000},  //2
                {000,128,000},  //3
                {000,255,255},  //4
                {128,000,064},  //5
                {255,000,000},  //6

                // Bombe
                {000,000,255},  //7

                //Covered Field
                {255,255,255},   //8

                //nicht Spielfeld
                {255,200,200},   //9

                //Flag
                {000,000,255},   //10

                //New Game Key
                {255,000,000},   //11

                //Game Lost background
                {000,000,255},   //12

                //Game Won background
                {000,255,255},   //13

                //New Game background
                {255,160,160},   //14

                //Bomb-Flag Counter
                {000,255,255},   //15
        };

        static public int Wins
        {
            get { return wins; }
            set
            {
                wins = value;
            }
        }

        static public int Total
        {
            get { return total; }
            set
            {
                total = value;
            }
        }

        static public int Losses
        {
            get { return losses; }
            set
            {
                losses = value;
            }
        }

        static public int Bombs
        {
            get { return bombs; }
            set
            {
                bombs = value;
            }
        }

        static public string KeyboardLayout
        {
            get { return keyboardLayout; }
            set
            {
                if (value != "DE" && value != "US")
                {
                    throw new Exception("Only German or Us layout allowed. (DE or US)");
                }
                keyboardLayout = value;
            }
        }

        #endregion

        #region Key Pressed

        //Handler for Key presses, gest passed number corresponding to field, from the intercept keys class
        static public void keyPressed(int i)
        {
            //Start Game If not Running
            if (!gameRunning)
            {
                // If not a static method, this.MainWindow would work
                main.UpdateStats();
                main.ResetWatch();
                newGame();
            }
            //Restart Game if plus is pressed
            else if (i == 99)
            {
                main.UpdateStats();
                main.StopWatchDefeat();
                main.ResetWatch();
                newGame();
            }
            //Dont take key press if Flag is present
            else if (display[(i % 11) + 1, (i / 11) + 1] == 10)
            {
            }
            else if (display[(i % 11) + 1, (i / 11) + 1] <= 6 && display[(i % 11) + 1, (i / 11) + 1] >= 0)
            {
                uncoverFlags(i % 11, i / 11);
            }
            //dont uncover bomb on first move
            else if (firstMove)
            {
                if (isBomb[(i % 11) + 1, (i / 11) + 1])
                {
                    MoveBomb((i % 11) + 1, (i / 11) + 1);
                }

                //start timer on first move
                main.UpdateStats();
                main.StartWatch();

                //add to total game counter
                total++;
                IncrementWinsBombs(42);

                firstMove = false;
                uncover(i % 11, i / 11);
                printLogiLED();
            }
            else
            {
                uncover(i % 11, i / 11);
                printLogiLED();
            }
        }

        #endregion

        #region New Game and Bomb Generation

        static public void newGame()
        {
            currentBack = 0;
            //damit man mit jeder taste starten kanN
            App.last = "Add";

            display = new int[21, 6];
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (i > 0 && i < 12 && j > 0 && j < 5)
                    {
                        display[i, j] = 8;
                        if (keyboardLayout == "US")
                        {
                            if (i == 1 && j == 4)
                            {
                                display[i, j] = 9;
                            }
                        }
                    }
                    else
                    {
                        display[i, j] = 9;
                    }
                }
            }

            if (keyboardLayout == "US")
            {
                covered = 43;
            }
            else
            {
                covered = 44;
            }

            firstMove = true;
            genBombs();

            genMap();
            isFlag = new bool[13, 6];

            colors[9, 0] = colors[14, 0];
            colors[9, 1] = colors[14, 1];
            colors[9, 2] = colors[14, 2];
            gameRunning = true;

            flagged = 0;

            setBackground = true;
            printLogiLED();
        }

        static private void MoveBomb(int x, int y)
        {
            Random r = new Random();
            for (int i = 1; i <= 1; i++)
            {
                int a = r.Next(1, isBomb.GetLength(0) - 1);
                int b = r.Next(1, isBomb.GetLength(1) - 1);
                if (keyboardLayout == "US")
                {
                    if (y == 4 && x == 1)
                    {
                        i--;
                        continue;
                    }
                }
                if (isBomb[a, b]) i--;
                else isBomb[a, b] = true;
            }

            isBomb[x, y] = false;
            genMap();
        }


        static private void genBombs()
        {
            isBomb = new bool[13, 6];
            Random r = new Random();
            for (int i = 1; i <= bombs; i++)
            {
                int x = r.Next(1, isBomb.GetLength(0) - 1);
                int y = r.Next(1, isBomb.GetLength(1) - 1);
                if (keyboardLayout == "US")
                {
                    if (y == 4 && x == 1)
                    {
                        i--;
                        continue;
                    }
                }
                if (isBomb[x, y]) i--;
                else isBomb[x, y] = true;
            }
        }

        static private void genMap()
        {
            map = new int[11, 4];

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (keyboardLayout == "US")
                    {
                        if (i == 0 && j == 3)
                        {
                            map[i, j] = 8;
                            continue;
                        }
                    }
                    if (isBomb[i + 1, j + 1]) map[i, j] = 7;
                    else
                    {
                        switch (j)
                        {
                            case 0:
                            case 1:
                                if (isBomb[i + 1, j]) map[i, j]++;
                                if (isBomb[i + 2, j]) map[i, j]++;
                                if (isBomb[i, j + 1]) map[i, j]++;
                                if (isBomb[i + 2, j + 1]) map[i, j]++;
                                if (isBomb[i, j + 2]) map[i, j]++;
                                if (isBomb[i + 1, j + 2]) map[i, j]++;
                                break;
                            case 2:
                                if (isBomb[i + 1, j]) map[i, j]++;
                                if (isBomb[i + 2, j]) map[i, j]++;
                                if (isBomb[i, j + 1]) map[i, j]++;
                                if (isBomb[i + 2, j + 1]) map[i, j]++;
                                if (isBomb[i + 1, j + 2]) map[i, j]++;
                                if (isBomb[i + 2, j + 2]) map[i, j]++;
                                break;
                            case 3:
                                if (isBomb[i, j]) map[i, j]++;
                                if (isBomb[i + 1, j]) map[i, j]++;
                                if (isBomb[i, j + 1]) map[i, j]++;
                                if (isBomb[i + 2, j + 1]) map[i, j]++;
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Print Board

        static private string print()
        {
            string s = "";
            for (int i = 0; i < display.GetLength(1); i++)
            {
                for (int j = 0; j < display.GetLength(0); j++)
                {
                    if (j == 0)
                    {
                        switch (i - 1)
                        {
                            case 0:
                                s += "";
                                break;
                            case 1:
                                s += " ";
                                break;
                            case 2:
                                s += "  ";
                                break;
                            case 3:
                                s += " ";
                                break;
                            default:
                                break;
                        }
                    }
                    if (display[j, i] == 7) s += "X ";
                    else if (display[j, i] == 8) s += "- ";
                    else s += display[j, i] + " ";
                }
                s += "\n";
            }
            return s;
        }

        static public void printLogiLED()
        {
            byte[] logiLED = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
            UpdateBackground();

            if (main._menuTabControl.SelectedIndex == 1)
            {
                printQwertz();
            }

            for (int i = 0; i < display.GetLength(1); i++)
            {
                for (int j = 0; j < display.GetLength(0); j++)
                {
                    colorToByte(logiLED, (i * 21 + j) * 4, colors[display[j, i], 0], colors[display[j, i], 1], colors[display[j, i], 2]);
                }
            }

            //LEGENDE
            //numPad 1-3 = 4*21 + 18-20
            colorToByte(logiLED, (5 * 21 + 18) * 4, colors[0, 0], colors[0, 1], colors[0, 2]);
            colorToByte(logiLED, (4 * 21 + 17) * 4, colors[1, 0], colors[1, 1], colors[1, 2]);
            colorToByte(logiLED, (4 * 21 + 18) * 4, colors[2, 0], colors[2, 1], colors[2, 2]);
            colorToByte(logiLED, (4 * 21 + 19) * 4, colors[3, 0], colors[3, 1], colors[3, 2]);
            //numPad 4-6 = 3*21 + 18-20
            colorToByte(logiLED, (3 * 21 + 17) * 4, colors[4, 0], colors[4, 1], colors[4, 2]);
            colorToByte(logiLED, (3 * 21 + 18) * 4, colors[5, 0], colors[5, 1], colors[5, 2]);
            colorToByte(logiLED, (3 * 21 + 19) * 4, colors[6, 0], colors[6, 1], colors[6, 2]);

            //New Game
            colorToByte(logiLED, 248, colors[11, 0], colors[11, 1], colors[11, 2]);

            //score
            for (int i = 0; i < bombs-flagged; i++)
            {
                if (i >= 12) break;
                colorToByte(logiLED, i * 4 + 4, colors[15,0], colors[15, 1], colors[15, 2]);
            }

            if (setBackground)
            {
                setBackground = false;
                LogitechGSDK.LogiLedSetLighting(Convert.ToInt32((Convert.ToDouble(colors[9, 2]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(colors[9, 1]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(colors[9, 0]) / 255.0) * 100));
            }
            LogitechGSDK.LogiLedSetLightingFromBitmap(logiLED);
        }

        static private void UpdateBackground()
        {
            if (currentBack == 0)
            {
                MineSweeper.colors[9, 0] = MineSweeper.colors[14, 0];
                MineSweeper.colors[9, 1] = MineSweeper.colors[14, 1];
                MineSweeper.colors[9, 2] = MineSweeper.colors[14, 2];
            }
            else if (currentBack == 1)
            {
                MineSweeper.colors[9, 0] = MineSweeper.colors[13, 0];
                MineSweeper.colors[9, 1] = MineSweeper.colors[13, 1];
                MineSweeper.colors[9, 2] = MineSweeper.colors[13, 2];
            }
            else
            {
                MineSweeper.colors[9, 0] = MineSweeper.colors[12, 0];
                MineSweeper.colors[9, 1] = MineSweeper.colors[12, 1];
                MineSweeper.colors[9, 2] = MineSweeper.colors[12, 2];
            }
        }

        static private void colorToByte(byte[] logiLED, int start, byte blue, byte green, byte red)
        {
            logiLED[start] = blue;
            logiLED[start + 1] = green;
            logiLED[start + 2] = red;
            logiLED[start + 3] = byte.MaxValue;
        }

        static public void printQwertz()
        {
            System.Windows.Shapes.Rectangle[] board = { main.h1, main.h2, main.h3, main.h4, main.h5, main.h6, main.h7, main.h8, main.h9, main.h10, main.h11, main.h12, main.h13, main.h14, main.h15, main.h16, main.h17, main.h18, main.h19, main.h20, main.h21, main.h22, main.h23, main.h24, main.h25, main.h26, main.h27, main.h28, main.h29, main.h30, main.h31, main.h32, main.h33, main.h34, main.h35, main.h36, main.h37, main.h38, main.h39, main.h40, main.h41, main.h42, main.h43, main.h44, };
            int counter = 0;
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 11; j++)
                {
                    board[counter++].Fill = new SolidColorBrush(Color.FromArgb(255, colors[display[j, i], 2], colors[display[j, i], 1], colors[display[j, i], 0]));
                }
            }

            switch (bombs - flagged)
            {
                case 1:
                    ShowKey(ref main.f1v); HideKey(ref main.f2); HideKey(ref main.f3); HideKey(ref main.f4); HideKey(ref main.f5); HideKey(ref main.f6); HideKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Hidden;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 52;
                    break;
                case 2:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); HideKey(ref main.f3); HideKey(ref main.f4); HideKey(ref main.f5); HideKey(ref main.f6); HideKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Hidden;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 106;
                    break;
                case 3:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); HideKey(ref main.f4); HideKey(ref main.f5); HideKey(ref main.f6); HideKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Hidden;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 160;
                    break;
                case 4:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); HideKey(ref main.f5); HideKey(ref main.f6); HideKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Hidden;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 214;
                    break;
                case 5:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); HideKey(ref main.f6); HideKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Visible;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 214;
                    main.function2.Width = 52;
                    break;
                case 6:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); ShowKey(ref main.f6); HideKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Visible;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 214;
                    main.function2.Width = 106;
                    break;
                case 7:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); ShowKey(ref main.f6); ShowKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Visible;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 214;
                    main.function2.Width = 160;
                    break;
                case 8:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); ShowKey(ref main.f6); ShowKey(ref main.f7); ShowKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Visible;
                    main.function3.Visibility = Visibility.Hidden;
                    main.function1.Width = 214;
                    main.function2.Width = 214;
                    break;
                case 9:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); ShowKey(ref main.f6); ShowKey(ref main.f7); ShowKey(ref main.f8); ShowKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Visible;
                    main.function3.Visibility = Visibility.Visible;
                    main.function1.Width = 214;
                    main.function2.Width = 214;
                    main.function3.Width = 52;
                    break;
                case 10:
                    ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); ShowKey(ref main.f6); ShowKey(ref main.f7); ShowKey(ref main.f8); ShowKey(ref main.f9); ShowKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Visible;
                    main.function3.Visibility = Visibility.Visible;
                    main.function1.Width = 214;
                    main.function2.Width = 214;
                    main.function3.Width = 106;
                    break;
                case 11:
                        ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); ShowKey(ref main.f6); ShowKey(ref main.f7); ShowKey(ref main.f8); ShowKey(ref main.f9); ShowKey(ref main.f10); ShowKey(ref main.f11); HideKey(ref main.f12);
                    main.function1.Visibility = Visibility.Visible;
                    main.function2.Visibility = Visibility.Visible;
                    main.function3.Visibility = Visibility.Visible;
                    main.function1.Width = 214;
                    main.function2.Width = 214;
                    main.function3.Width = 160;
                    break;
                default:
                    if((bombs - flagged) > 11)
                    {
                        ShowKey(ref main.f1v); ShowKey(ref main.f2); ShowKey(ref main.f3); ShowKey(ref main.f4); ShowKey(ref main.f5); ShowKey(ref main.f6); ShowKey(ref main.f7); ShowKey(ref main.f8); ShowKey(ref main.f9); ShowKey(ref main.f10); ShowKey(ref main.f11); ShowKey(ref main.f12);
                        main.function1.Visibility = Visibility.Visible;
                        main.function2.Visibility = Visibility.Visible;
                        main.function3.Visibility = Visibility.Visible;
                        main.function1.Width = 214;
                        main.function2.Width = 214;
                        main.function3.Width = 214;
                    }
                    else
                    {
                        HideKey(ref main.f1v); HideKey(ref main.f2); HideKey(ref main.f3); HideKey(ref main.f4); HideKey(ref main.f5); HideKey(ref main.f6); HideKey(ref main.f7); HideKey(ref main.f8); HideKey(ref main.f9); HideKey(ref main.f10); HideKey(ref main.f11); HideKey(ref main.f12);
                        main.function1.Visibility = Visibility.Hidden;
                        main.function2.Visibility = Visibility.Hidden;
                        main.function3.Visibility = Visibility.Hidden;
                    }
                    break;
            }

            main.esc1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[9, 2], MineSweeper.colors[9, 1], MineSweeper.colors[9, 0]));
        }

        static private void ShowKey(ref System.Windows.Controls.Canvas a)
        {
            a.Visibility = Visibility.Visible;
        }

        static private void HideKey(ref System.Windows.Controls.Canvas a)
        {
            a.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Game Logic

        static public void SetFlag(int i)
        {
            //event handler for newgame because it calls setflag wenn shift is pressed so you can restart with pressed shift
            if (i == 99)
            {
                main.UpdateStats();
                main.StopWatchDefeat();
                main.ResetWatch();
                newGame();
            }
            else if (!gameRunning)
            {
                // If not a static method, this.MainWindow would work
                main.UpdateStats();
                main.ResetWatch();
                newGame();
            }
            //take away flag if already present
            else if (display[(i % 11) + 1, (i / 11) + 1] == 10)
            {
                display[(i % 11) + 1, (i / 11) + 1] = 8;
                isFlag[(i % 11) + 1, (i / 11) + 1] = false;
                flagged--;
            }
            //place flag if field is empty
            else if (display[(i % 11) + 1, (i / 11) + 1] == 8)
            {
                display[(i % 11) + 1, (i / 11) + 1] = 10;
                isFlag[(i % 11) + 1, (i / 11) + 1] = true;
                flagged++;
            }
            printLogiLED();
        }

        static private void uncover(int x, int y)
        {
            //stop if x or y are out of range
            if (x >= map.GetLength(0) || y >= map.GetLength(1) || x < 0 || y < 0) return;
            //instant return if already ucovered
            if (display[x + 1, y + 1] != 8) return;

            //set m to value of the bomb map
            int m = map[x, y];

            //
            if (m < 8 && m >= 0)
            {
                display[x + 1, y + 1] = m;

                //so you can press flag right after starting
                App.last = "empty";

                if (--covered <= bombs && m != 7)
                {
                    victory();
                    return;
                }
            }
            //7 is if a field is a bomb
            if (m == 7)
            {
                colors[9, 0] = colors[12, 0];
                colors[9, 1] = colors[12, 1];
                colors[9, 2] = colors[12, 2];

                //stop timer defeat and increment losses
                main.UpdateStats();
                main.StopWatchDefeat();
                currentBack = 2;

                losses++;
                IncrementWinsBombs(21);

                gameOver();
            }
            //if empty recursively call funtion from all surrounding fields, if also empty recursively calls again
            else if (m == 0)
            {
                switch (y)
                {
                    case 0:
                    case 1:
                        uncover(x, y - 1);
                        uncover(x + 1, y - 1);
                        uncover(x - 1, y);
                        uncover(x + 1, y);
                        uncover(x - 1, y + 1);
                        uncover(x, y + 1);
                        break;
                    case 2:
                        uncover(x, y - 1);
                        uncover(x + 1, y - 1);
                        uncover(x - 1, y);
                        uncover(x + 1, y);
                        uncover(x, y + 1);
                        uncover(x + 1, y + 1);
                        break;
                    case 3:
                        uncover(x - 1, y - 1);
                        uncover(x, y - 1);
                        uncover(x - 1, y);
                        uncover(x + 1, y);
                        break;
                }
            }
        }

        //function for uncovering when all surrounding bombs of field are flagged, or atleast right amount
        static private void uncoverFlags(int x, int y)
        {
            int sourroundingFlags = 0;
            switch (y)
            {
                case 0:
                case 1:
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    if (isFlag[x + 2, y]) sourroundingFlags++;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    if (isFlag[x, y + 2]) sourroundingFlags++;
                    if (isFlag[x + 1, y + 2]) sourroundingFlags++;
                    break;
                case 2:
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    if (isFlag[x + 2, y]) sourroundingFlags++;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 1, y + 2]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 2]) sourroundingFlags++;
                    break;
                case 3:
                    if (isFlag[x, y]) sourroundingFlags++;
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    break;
            }

            if (display[x + 1, y + 1] <= sourroundingFlags)
            {
                switch (y)
                {
                    case 0:
                    case 1:
                        if (!isFlag[x + 1, y]) uncover(x, y - 1);
                        if (!isFlag[x + 2, y]) uncover(x + 1, y - 1);
                        if (!isFlag[x, y + 1]) uncover(x - 1, y);
                        if (!isFlag[x + 2, y + 1]) uncover(x + 1, y);
                        if (!isFlag[x, y + 2]) uncover(x - 1, y + 1);
                        if (!isFlag[x + 1, y + 2]) uncover(x, y + 1);
                        break;
                    case 2:
                        if (!isFlag[x + 1, y]) uncover(x, y - 1);
                        if (!isFlag[x + 2, y]) uncover(x + 1, y - 1);
                        if (!isFlag[x, y + 1]) uncover(x - 1, y);
                        if (!isFlag[x + 2, y + 1]) uncover(x + 1, y);
                        if (!isFlag[x + 1, y + 2]) uncover(x, y + 1);
                        if (!isFlag[x + 2, y + 2]) uncover(x + 1, y + 1);
                        break;
                    case 3:
                        if (!isFlag[x, y]) uncover(x - 1, y - 1);
                        if (!isFlag[x + 1, y]) uncover(x, y - 1);
                        if (!isFlag[x, y + 1]) uncover(x - 1, y);
                        if (!isFlag[x + 2, y + 1]) uncover(x + 1, y);
                        break;
                }
            }

            printLogiLED();
        }

        #endregion

        #region Game End Functions

        static private void gameOver()
        {
            main.UpdateStats();

            //so you cant spam new game
            App.last = "empty";
            for (int i = 0; i < isBomb.GetLength(0); i++)
            {
                for (int j = 0; j < isBomb.GetLength(1); j++)
                {
                    if (isBomb[i, j]) display[i, j] = 7;

                }
            }

            setBackground = true;
            printLogiLED();

            gameRunning = false;
        }

        static private void victory()
        {
            //stop timer victory
            wins++;

            IncrementWinsBombs(0);
            
            main.UpdateStats();
            main.StopWatchVictory();
            currentBack = 1;
            colors[9, 0] = colors[13, 0];
            colors[9, 1] = colors[13, 1];
            colors[9, 2] = colors[13, 2];

            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string[] lines = { "Wins: " + wins, "Bombs: " + bombs, "Layout: " + keyboardLayout, "Total: " + total, "Losses: " + losses };
            var file = Path.Combine(systemPath, "Logitech MineSweeper/config.txt");
            File.WriteAllLines(file, lines);

            gameOver();
        }

        #endregion

        #region Misc

        static private void IncrementWinsBombs(int add)
        {
            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var directory = Path.Combine(systemPath, "Logitech MineSweeper");

            var file = "a";

            if (keyboardLayout == "US")
            {
                file = Path.Combine(systemPath, "Logitech MineSweeper/US.txt");
            }
            else
            {
                file = Path.Combine(systemPath, "Logitech MineSweeper/DE.txt");
            }

            string[] US = File.ReadAllLines(file);
            
            int line = Convert.ToInt32(US[bombs + 21 + add]) + 1;

            US[bombs + 21 + add] = line.ToString();
            
            File.WriteAllLines(file, US);

            try
            {
                main.UpdateStats();
            }
            catch
            {

            }
        }

        #endregion
    }
}
