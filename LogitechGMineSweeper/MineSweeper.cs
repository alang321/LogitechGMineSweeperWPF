using System;
using System.Diagnostics;

namespace LogitechGMineSweeper
{
    class MineSweeper
    {
        #region Variables Constructor and Properties

        public enum GameStateEnum { Default, Victory, Defeat }

        public MineSweeper(SaveFileSettings settings, SaveFileGlobalStatistics globalStats, KeyboardLayout keyLayout, SaveFileColors ColorsFile)
        {
            this.ColorsFile = ColorsFile;
            this.Colors = ColorsFile.SavedColors;
            this.Settings = settings;
            this.KeyboardLayout = keyLayout;
            this.GlobalStats = globalStats;

            NewGame();
        }

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

        public SaveFileSettings Settings { get; set; }
        public SaveFileGlobalStatistics GlobalStats { get; set; }
        public SaveFileColors ColorsFile { get; set; }

        //local settings variables
        KeyboardLayout keyboardLayout;

        int[,] map;
        bool[,] isBomb = new bool[14, 6];
        bool[,] isFlag = new bool[14, 6];
        int[,] display;
        bool gameRunning;
        bool firstMove = true;
        bool setBackground = false;
        Random r = new Random();
        public int GameState { get; private set; } = 0;
        public static bool KeyboardDisplayShown { get; set; } = false;

        int[] availeableBombField;
        int availeableBombFieldCounter;

        int covered;
        int flagged = 0;

        //values actually not used anymore just read out of file, also values not up to date, just to see what each value is
        public byte[,] Colors { get; set; } = new byte[17,3];
        // 0 = bombs sourrounding counter
        // 1 = bombs sourrounding counter
        // 2 = bombs sourrounding counter
        // 3 = bombs sourrounding counter
        // 4 = bombs sourrounding counter
        // 5 = bombs sourrounding counter
        // 6 = bombs sourrounding counter
        // 7 = Bombe
        // 8 = Covered Field
        // 9 = placeholder for the other background Colors
        // 10 = Flag
        // 11 = New Game Key
        // 12 = Game Lost background
        // 13 = Game Won background
        // 14 = New Game background
        // 15 = Bomb-Flag Counter
        // 16 = Flag Key Color

        public int[,] Display
        {
            get { return display; }
            set
            {
                display = value;
            }
        }

        public int Bombs
        {
            get { return Settings.Bombs; }
            set
            {
                Settings.Bombs = value;
            }
        }

        public int Wins
        {
            get { return GlobalStats.Wins; }
            set
            {
                GlobalStats.Wins = value;
            }
        }

        public int Losses
        {
            get { return GlobalStats.Losses; }
            set
            {
                GlobalStats.Losses = value;
            }
        }

        public int Total
        {
            get { return GlobalStats.Total; }
            set
            {
                GlobalStats.Total = value;
            }
        }

        public bool UseBackground
        {
            get { return Settings.UseBackground; }
            set
            {
                Settings.UseBackground = value;
            }
        }

        public int Flagged
        {
            get { return flagged; }
        }

        public KeyboardLayout KeyboardLayout
        {
            get { return keyboardLayout; }
            set
            {
                keyboardLayout = value;
                Settings.LayoutIndex = keyboardLayout.Index;
            }
        }


        #endregion

        #region Key Pressed

        //Handler for Key presses, gest passed number corresponding to field, from the intercept keys class
        public void KeyPressed(int i)
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
                UncoverFlags(i % 12, i / 12);
            }
            //dont Uncover bomb on first move
            else if (firstMove)
            {
                GenBombs(i / 12, i % 12);

                //start timer on first move
                UpdateStatsEvent();
                StartWatchEvent();

                //add to total game counter
                GlobalStats.Total++;

                keyboardLayout.SaveFile.IncrementTotal(Settings.Bombs);

                firstMove = false;
                Uncover(i % 12, i / 12);
                PrintLogiLED();
            }
            else
            {
                Uncover(i % 12, i / 12);
                PrintLogiLED();
            }
        }

        public void SetFlag(int i)
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
            PrintLogiLED();
        }

        #endregion

        #region New Game and Bomb Generation

        public void NewGame()
        {
            GameState = (int)GameStateEnum.Default;

            //so you cant start with every key
            App.last = 107;

            ResetDisplay();

            covered = keyboardLayout.CoveredFields;

            //so timer can be started when key i spressed and firstmove is true
            firstMove = true;

            isFlag = new bool[14, 6];
            flagged = 0;

            gameRunning = true;
            
            setBackground = true;

            PrintLogiLED();
        }

        private void GenBombs(int x, int y)
        {
            isBomb = new bool[14, 6];
            availeableBombField = new int[48];
            availeableBombFieldCounter = 0;

            for (int i = 0; i < keyboardLayout.EnabledKeys.GetLength(0); i++)
            {
                for (int j = 0; j < keyboardLayout.EnabledKeys.GetLength(1); j++)
                {
                    if ((i != x || j != y) && keyboardLayout.EnabledKeys[i, j]) availeableBombField[availeableBombFieldCounter++] = i * keyboardLayout.EnabledKeys.GetLength(1) + j;
                }
            }

            for(int i = 0; i < Settings.Bombs; i++)
            {
                int index = r.Next(0, availeableBombFieldCounter);
                isBomb[(availeableBombField[index] % 12) + 1,(availeableBombField[index] / 12) + 1] = true;
                availeableBombFieldCounter--;
                availeableBombField[index] = availeableBombField[availeableBombFieldCounter];
            }

            GenMap();
        }

        private void ResetDisplay()
        {
            display = new int[21, 6];
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (i > 0 && i < 13 && j > 0 && j < 5)
                    {
                        if (!keyboardLayout.EnabledKeys[j - 1, i - 1])
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

        private void GenMap()
        {
            map = new int[12, 4];

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (!keyboardLayout.EnabledKeys[j, i])
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

        private string PrintDisplay()
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

        private string PrintBombs()
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

        private string PrintMap()
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

        public void PrintLogiLED()
        {
            PrintLogiLED(true);
        }

        public void PrintLogiLED(bool printDisplay)
        {
            //init bitmap that will be used to create light
            byte[] logiLED = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];

            //implemented background change on loss later so the whole thing is pretty messy so this is necessary
            UpdateBackground();

            //only print the in-app keyboard when the tab is selected
            if (MineSweeper.KeyboardDisplayShown && printDisplay)
            {
                PrintEvent();
            }
            
            Debug.WriteLine(PrintBombs());

            //for actually printing the board
            for (int i = 0; i < display.GetLength(1); i++)
            {
                for (int j = 0; j < display.GetLength(0); j++)
                {
                    ColorToByte(logiLED, (i * 21 + j) * 4, Colors[display[j, i], 0], Colors[display[j, i], 1], Colors[display[j, i], 2]);
                }
            }

            //LEGENDE
            //numPad 1-3 = 4*21 + 18-20
            ColorToByte(logiLED, (5 * 21 + 18) * 4, Colors[0, 0], Colors[0, 1], Colors[0, 2]);
            ColorToByte(logiLED, (4 * 21 + 17) * 4, Colors[1, 0], Colors[1, 1], Colors[1, 2]);
            ColorToByte(logiLED, (4 * 21 + 18) * 4, Colors[2, 0], Colors[2, 1], Colors[2, 2]);
            ColorToByte(logiLED, (4 * 21 + 19) * 4, Colors[3, 0], Colors[3, 1], Colors[3, 2]);
            //numPad 4-6 = 3*21 + 18-20
            ColorToByte(logiLED, (3 * 21 + 17) * 4, Colors[4, 0], Colors[4, 1], Colors[4, 2]);
            ColorToByte(logiLED, (3 * 21 + 18) * 4, Colors[5, 0], Colors[5, 1], Colors[5, 2]);
            ColorToByte(logiLED, (3 * 21 + 19) * 4, Colors[6, 0], Colors[6, 1], Colors[6, 2]);

            //shift keys
            if (Settings.UseBackground)
            {
                ColorToByte(logiLED, (4 * 21 + 0) * 4, Colors[9, 0], Colors[9, 1], Colors[9, 2]);
                ColorToByte(logiLED, (4 * 21 + 13) * 4, Colors[9, 0], Colors[9, 1], Colors[9, 2]);
            }
            else
            {
                ColorToByte(logiLED, (4 * 21 + 0) * 4, Colors[16, 0], Colors[16, 1], Colors[16, 2]);
                ColorToByte(logiLED, (4 * 21 + 13) * 4, Colors[16, 0], Colors[16, 1], Colors[16, 2]);
            }

            //New Game
            ColorToByte(logiLED, 248, Colors[11, 0], Colors[11, 1], Colors[11, 2]);

            //bomb counter
            if (GameState == (int)GameStateEnum.Default)
            {
                for (int i = 0; i < Settings.Bombs-flagged; i++)
                {
                    if (i >= 12) break;
                    ColorToByte(logiLED, i * 4 + 4, Colors[15,0], Colors[15, 1], Colors[15, 2]);
                }
            }

            //disabled
            //bool trigger for setting background as it would shortly flash if set every time
            if (Config.SetLogiLogo && setBackground)
            {
                setBackground = false;
                LogitechGSDK.LogiLedSetLighting(Convert.ToInt32((Convert.ToDouble(Colors[9, 2]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(Colors[9, 1]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(Colors[9, 0]) / 255.0) * 100));
            }

            //display the new color
            LogitechGSDK.LogiLedSetLightingFromBitmap(logiLED);
        }

        private void UpdateBackground()
        {
            if (GameState == (int)GameStateEnum.Default)
            {
                Colors[9, 0] = Colors[14, 0];
                Colors[9, 1] = Colors[14, 1];
                Colors[9, 2] = Colors[14, 2];
            }
            else if (GameState == (int)GameStateEnum.Victory)
            {
                Colors[9, 0] = Colors[13, 0];
                Colors[9, 1] = Colors[13, 1];
                Colors[9, 2] = Colors[13, 2];
            }
            else
            {
                Colors[9, 0] = Colors[12, 0];
                Colors[9, 1] = Colors[12, 1];
                Colors[9, 2] = Colors[12, 2];
            }
        }

        static private void ColorToByte(byte[] logiLED, int start, byte blue, byte green, byte red)
        { 
            //for getting the in app map to the required format, alpha is set to max
            logiLED[start] = blue;
            logiLED[start + 1] = green;
            logiLED[start + 2] = red;
            logiLED[start + 3] = byte.MaxValue;
        }

        #endregion

        #region Game Logic

        private void Uncover(int x, int y)
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

                if (--covered <= Settings.Bombs && m != 7)
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
                        Uncover(x, y - 1);
                        Uncover(x + 1, y - 1);
                        Uncover(x - 1, y);
                        Uncover(x + 1, y);
                        Uncover(x - 1, y + 1);
                        Uncover(x, y + 1);
                        break;
                    case 2:
                        Uncover(x, y - 1);
                        Uncover(x + 1, y - 1);
                        Uncover(x - 1, y);
                        Uncover(x + 1, y);
                        Uncover(x, y + 1);
                        Uncover(x + 1, y + 1);
                        break;
                    case 3:
                        Uncover(x - 1, y - 1);
                        Uncover(x, y - 1);
                        Uncover(x - 1, y);
                        Uncover(x + 1, y);
                        break;
                }
            }
        }

        //function for Uncovering when all surrounding bombs of field are flagged, or atleast right amount
        private void UncoverFlags(int x, int y)
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
                            if (!isFlag[x + 1, y]) Uncover(x, y - 1);
                            if (!isFlag[x + 2, y]) Uncover(x + 1, y - 1);
                            if (!isFlag[x, y + 1]) Uncover(x - 1, y);
                            if (!isFlag[x + 2, y + 1]) Uncover(x + 1, y);
                            if (!isFlag[x, y + 2]) Uncover(x - 1, y + 1);
                            if (!isFlag[x + 1, y + 2]) Uncover(x, y + 1);
                            break;
                        case 2:
                            if (!isFlag[x + 1, y]) Uncover(x, y - 1);
                            if (!isFlag[x + 2, y]) Uncover(x + 1, y - 1);
                            if (!isFlag[x, y + 1]) Uncover(x - 1, y);
                            if (!isFlag[x + 2, y + 1]) Uncover(x + 1, y);
                            if (!isFlag[x + 1, y + 2]) Uncover(x, y + 1);
                            if (!isFlag[x + 2, y + 2]) Uncover(x + 1, y + 1);
                            break;
                        case 3:
                            if (!isFlag[x, y]) Uncover(x - 1, y - 1);
                            if (!isFlag[x + 1, y]) Uncover(x, y - 1);
                            if (!isFlag[x, y + 1]) Uncover(x - 1, y);
                            if (!isFlag[x + 2, y + 1]) Uncover(x + 1, y);
                            break;
                    }
                    PrintLogiLED();
                }
            }
        }

        #endregion

        #region Game End Functions

        private void GameOver()
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
            PrintLogiLED();

            gameRunning = false;
        }

        private void Victory()
        {
            GlobalStats.Wins++;

            keyboardLayout.SaveFile.IncrementWins(Settings.Bombs);

            StopWatchVictoryEvent();

            GameState = (int)GameStateEnum.Victory;

            GameOver();
        }

        private void Defeat()
        {
            GlobalStats.Losses++;

            keyboardLayout.SaveFile.IncrementLosses(Settings.Bombs);

            StopWatchDefeatEvent();

            GameState = (int)GameStateEnum.Defeat;

            GameOver();
        }

        #endregion
    }
}
