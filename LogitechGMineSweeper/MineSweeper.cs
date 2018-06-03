using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls;

namespace LogitechGMineSweeper
{
    static class MineSweeper
    {
        #region Variables Constructor and Properties

        public delegate void PrintdisplayEventHandler();
        public delegate void UpdateStatsEventHandler();
        public delegate void StopWatchDefeatEventHandler();
        public delegate void StopWatchVictoryEventHandler();
        public delegate void StartWatchEventHandler();
        public delegate void ResetWatchEventHandler();

        public static event PrintdisplayEventHandler PrintEvent;
        public static event UpdateStatsEventHandler UpdateStatsEvent;
        public static event StopWatchDefeatEventHandler StopWatchDefeatEvent;
        public static event StopWatchVictoryEventHandler StopWatchVictoryEvent;
        public static event StartWatchEventHandler StartWatchEvent;
        public static event ResetWatchEventHandler ResetWatchEvent;

        static int[,] map;
        static bool[,] isBomb;
        static bool[,] isFlag = new bool[14, 6];
        private static int[,] display;
        static int bombs = 13;
        static bool gameRunning;
        static bool firstMove = true;
        static int keyboardLayout = (int)Config.Layout.DE;
        static bool setBackground = false;
        static Random r = new Random();
        static public int GameState { get; private set; } = 0;
        static public bool KeyboardDisplayShown { get; set; } = false;

        static int[] availeableBombField;
        static int availeableBombFieldCounter;

        //covered key count for current layout
        static int coveredReset;

        static int covered = 11 * 4;
        static int flagged = 0;

        public enum GameStateEnum { Default, Victory, Defeat }

        //values actually not used anymore just read out of file, also values not up to date, just to see what each value is
        public static byte[,] colors =
            {
                // bombs sourrounding counter
                {000,000,000},  //0
                {128, 000, 128},  //1
                {255,255,000},  //2
                {000,128,000},  //3
                {000,255,255},  //4
                {000, 127, 255},  //5
                {255,000,000},  //6

                // Bombe
                {000,000,255},  //7

                //Covered Field
                {255,255,255},   //8

                //nicht Spielfeld
                {255,200,200},   //9

                //Flag
                {255,000,255},   //10

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

                //Flag Key Color
                {255,000,255},   //16
        };
        
        static public int[,] Display
        {
            get { return display; }
            set
            {
                display = value;
            }
        }

        static public int Flagged
        {
            get { return flagged; }
        }

        static public int Bombs
        {
            get { return bombs; }
            set
            {
                if (value >= Config.minBombs && value <= Config.maxBombs)
                {
                    bombs = value;
                }
                else
                {
                    throw new Exception("Amount of Bombs not supported!");
                }
            }
        }

        static public int KeyboardLayout
        {
            get { return keyboardLayout; }
            set
            {
                keyboardLayout = value;

                coveredReset = 0;

                for (int i = 0; i < Config.KeyboardLayouts[keyboardLayout].EnabledKeys.GetLength(0); i++)
                {
                    for (int j = 0; j < Config.KeyboardLayouts[keyboardLayout].EnabledKeys.GetLength(1); j++)
                    {
                        if (Config.KeyboardLayouts[keyboardLayout].EnabledKeys[i,j]) coveredReset++;
                    }
                }
            }
        }

        #endregion

        #region Key Pressed

        //Handler for Key presses, gest passed number corresponding to field, from the intercept keys class
        static public void KeyPressed(int i)
        {
            //Start Game If not Running
            if (!gameRunning)
            {
                UpdateStatsEvent();
                ResetWatchEvent();
                NewGame();
            }
            //Restart Game if plus is pressed
            else if (i == 48)
            {
                UpdateStatsEvent();
                StopWatchDefeatEvent();
                ResetWatchEvent();
                NewGame();
            }
            //Dont take key press if Flag is present
            else if (display[(i % 12) + 1, (i / 12) + 1] == 10)
            {
                return;
            }
            else if (display[(i % 12) + 1, (i / 12) + 1] <= 6 && display[(i % 12) + 1, (i / 12) + 1] >= 0)
            {
                uncoverFlags(i % 12, i / 12);
            }
            //dont uncover bomb on first move
            else if (firstMove)
            {
                if (isBomb[(i % 12) + 1, (i / 12) + 1])
                {
                    MoveBomb((i % 12) + 1, (i / 12) + 1);
                }

                //start timer on first move
                UpdateStatsEvent();
                StartWatchEvent();

                //add to total game counter
                Config.fileConfig.Total += 1;

                Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.IncrementTotal(bombs);

                firstMove = false;
                uncover(i % 12, i / 12);
                printLogiLED();
            }
            else
            {
                uncover(i % 12, i / 12);
                printLogiLED();
            }
        }

        static public void SetFlag(int i)
        {
            //event handler for newgame because it calls setflag wenn shift is pressed so you can restart with pressed shift
            if (i == 48)
            {
                UpdateStatsEvent();
                StopWatchDefeatEvent();
                ResetWatchEvent();
                NewGame();
            }
            else if (!gameRunning)
            {
                UpdateStatsEvent();
                ResetWatchEvent();
                NewGame();
            }
            //take away flag if already present
            else if (display[(i % 12) + 1, (i / 12) + 1] == 10)
            {
                display[(i % 12) + 1, (i / 12) + 1] = 8;
                isFlag[(i % 12) + 1, (i / 12) + 1] = false;
                flagged--;
            }
            //place flag if field is empty
            else if (display[(i % 12) + 1, (i / 12) + 1] == 8)
            {
                display[(i % 12) + 1, (i / 12) + 1] = 10;
                isFlag[(i % 12) + 1, (i / 12) + 1] = true;
                flagged++;
            }
            printLogiLED();
        }

        #endregion

        #region New Game and Bomb Generation

        static public void NewGame()
        {
            GameState = (int)GameStateEnum.Default;

            //so you cant start with every key
            App.last = 107;

            ResetDisplay();

            covered = coveredReset;

            //so timer can be started when key i spressed and firstmove is true
            firstMove = true;

            GenBombs();
            GenMap();

            isFlag = new bool[14, 6];
            flagged = 0;

            gameRunning = true;
            
            setBackground = true;

            printLogiLED();
        }

        static private void MoveBomb(int x, int y)
        {
            int index = r.Next(0, availeableBombFieldCounter);
            isBomb[(availeableBombField[index] % 12) + 1, (availeableBombField[index] / 12) + 1] = true;
            availeableBombFieldCounter--;
            availeableBombField[index] = availeableBombField[availeableBombFieldCounter];

            isBomb[x, y] = false;
            GenMap();
        }

        static private void GenBombs()
        {
            isBomb = new bool[14, 6];
            availeableBombField = new int[48];
            availeableBombFieldCounter = 0;

            for(int i = 0; i < Config.KeyboardLayouts[MineSweeper.keyboardLayout].EnabledKeys.GetLength(0); i++)
            {
                for (int j = 0; j < Config.KeyboardLayouts[MineSweeper.keyboardLayout].EnabledKeys.GetLength(1); j++)
                {
                    if (Config.KeyboardLayouts[MineSweeper.keyboardLayout].EnabledKeys[i, j]) availeableBombField[availeableBombFieldCounter++] = i * Config.KeyboardLayouts[MineSweeper.keyboardLayout].EnabledKeys.GetLength(1) + j;
                }
            }

            for(int i = 0; i < bombs; i++)
            {
                int index = r.Next(0, availeableBombFieldCounter);
                isBomb[(availeableBombField[index] % 12) + 1,(availeableBombField[index] / 12) + 1] = true;
                availeableBombFieldCounter--;
                availeableBombField[index] = availeableBombField[availeableBombFieldCounter];
            }
        }

        static private void ResetDisplay()
        {
            display = new int[21, 6];
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (i > 0 && i < 13 && j > 0 && j < 5)
                    {
                        if (!Config.KeyboardLayouts[keyboardLayout].EnabledKeys[j - 1, i - 1])
                        {
                            display[i, j] = 9;
                        }
                        else
                        {
                            display[i, j] = 8;
                        }
                    }
                    else
                    {
                        display[i, j] = 9;
                    }
                }
            }
        }

        static private void GenMap()
        {
            map = new int[12, 4];

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (!Config.KeyboardLayouts[keyboardLayout].EnabledKeys[j, i])
                    {
                        map[i, j] = 8;
                        continue;
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

        #region Debug

        static private string printDisplay()
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

        static private string printBombs()
        {
            string s = "";
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 12; j++)
                {
                    if (j == 1)
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
                    if (isBomb[j, i]) s += "X ";
                    else if (!isBomb[j, i]) s += "- ";
                }
                s += "\n";
            }
            return s;
        }

        static private string printMap()
        {
            string s = "";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    if (j == 0)
                    {
                        switch (i)
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
                    if(map[j, i] == 7)
                    {
                        s += "X ";
                    }
                    else
                    {
                        s += map[j, i].ToString() + " ";
                    }
                }
                s += "\n";
            }
            return s;
        }

        #endregion

        static public void printLogiLED()
        {
            printLogiLED(true);
        }

        static public void printLogiLED(bool printDisplay)
        {
            //init bitmap that will be used to create light
            byte[] logiLED = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];

            //implemented background change on loss later so the whole thing is pretty messy so this is necessary
            UpdateBackground();

            //only print the in-app keyboard when the tab is selected
            if (KeyboardDisplayShown && printDisplay)
            {
                PrintEvent();
            }

            Debug.WriteLine(printBombs());

            //for actually printing the board
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

            //shift keys
            if (Config.fileConfig.UseBackground)
            {
                colorToByte(logiLED, (4 * 21 + 0) * 4, colors[9, 0], colors[9, 1], colors[9, 2]);
                colorToByte(logiLED, (4 * 21 + 13) * 4, colors[9, 0], colors[9, 1], colors[9, 2]);
            }
            else
            {
                colorToByte(logiLED, (4 * 21 + 0) * 4, colors[16, 0], colors[16, 1], colors[16, 2]);
                colorToByte(logiLED, (4 * 21 + 13) * 4, colors[16, 0], colors[16, 1], colors[16, 2]);
            }

            //New Game
            colorToByte(logiLED, 248, colors[11, 0], colors[11, 1], colors[11, 2]);

            //bomb counter
            if (GameState == (int)GameStateEnum.Default)
            {
                for (int i = 0; i < bombs-flagged; i++)
                {
                    if (i >= 12) break;
                    colorToByte(logiLED, i * 4 + 4, colors[15,0], colors[15, 1], colors[15, 2]);
                }
            }

            //disabled
            //bool trigger for setting background as it would shortly flash if set every time
            if (Config.setLogiLogo && setBackground)
            {
                setBackground = false;
                LogitechGSDK.LogiLedSetLighting(Convert.ToInt32((Convert.ToDouble(colors[9, 2]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(colors[9, 1]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(colors[9, 0]) / 255.0) * 100));
            }

            //display the new color
            LogitechGSDK.LogiLedSetLightingFromBitmap(logiLED);
        }

        static private void UpdateBackground()
        {
            if (GameState == (int)GameStateEnum.Default)
            {
                MineSweeper.colors[9, 0] = MineSweeper.colors[14, 0];
                MineSweeper.colors[9, 1] = MineSweeper.colors[14, 1];
                MineSweeper.colors[9, 2] = MineSweeper.colors[14, 2];
            }
            else if (GameState == (int)GameStateEnum.Victory)
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
            //for getting the in app map to the required format, alpha is set to max
            logiLED[start] = blue;
            logiLED[start + 1] = green;
            logiLED[start + 2] = red;
            logiLED[start + 3] = byte.MaxValue;
        }

        #endregion

        #region Game Logic

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

                App.last = -1;

                if (--covered <= bombs && m != 7)
                {
                    Victory();
                    return;
                }
            }
            //7 is if a field is a bomb
            if (m == 7)
            {
                Defeat();
            }
            //if empty recursively call funtion from all surrounding fields
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
            bool defeatIfUncover = false;
            switch (y)
            {
                case 0:
                case 1:
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    else if(isBomb[x + 1, y]) defeatIfUncover = true;
                    if (isFlag[x + 2, y]) sourroundingFlags++;
                    else if (isBomb[x + 2, y]) defeatIfUncover = true;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    else if (isBomb[x, y + 1]) defeatIfUncover = true;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    else if (isBomb[x + 2, y + 1]) defeatIfUncover = true;
                    if (isFlag[x, y + 2]) sourroundingFlags++;
                    else if (isBomb[x, y + 2]) defeatIfUncover = true;
                    if (isFlag[x + 1, y + 2]) sourroundingFlags++;
                    else if (isBomb[x + 1, y + 2]) defeatIfUncover = true;
                    break;
                case 2:
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    else if (isBomb[x + 1, y]) defeatIfUncover = true;
                    if (isFlag[x + 2, y]) sourroundingFlags++;
                    else if (isBomb[x + 2, y]) defeatIfUncover = true;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    else if (isBomb[x, y + 1]) defeatIfUncover = true;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    else if (isBomb[x + 2, y + 1]) defeatIfUncover = true;
                    if (isFlag[x + 1, y + 2]) sourroundingFlags++;
                    else if (isBomb[x + 1, y + 2]) defeatIfUncover = true;
                    if (isFlag[x + 2, y + 2]) sourroundingFlags++;
                    else if (isBomb[x + 2, y + 2]) defeatIfUncover = true;
                    break;
                case 3:
                    if (isFlag[x, y]) sourroundingFlags++;
                    else if (isBomb[x, y]) defeatIfUncover = true;
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    else if (isBomb[x + 1, y]) defeatIfUncover = true;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    else if (isBomb[x, y + 1]) defeatIfUncover = true;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    else if (isBomb[x + 2, y + 1]) defeatIfUncover = true;
                    break;
            }

            if (display[x + 1, y + 1] <= sourroundingFlags)
            {
                if (defeatIfUncover)
                {
                    Defeat();
                }
                else
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
                    printLogiLED();
                }
            }
        }

        #endregion

        #region Game End Functions

        static private void GameOver()
        {
            UpdateStatsEvent();

            //so you cant spam new game
            App.last = -1;

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

        static private void Victory()
        {
            Config.fileConfig.Wins++;

            Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.IncrementWins(bombs);

            StopWatchVictoryEvent();

            GameState = (int)GameStateEnum.Victory;

            GameOver();
        }

        static private void Defeat()
        {
            Config.fileConfig.Losses++;

            Config.KeyboardLayouts[MineSweeper.KeyboardLayout].SaveFile.IncrementLosses(bombs);

            StopWatchDefeatEvent();

            GameState = (int)GameStateEnum.Defeat;

            GameOver();
        }

        #endregion
    }
}
