using System;
using System.Diagnostics;

namespace LogitechGMineSweeper
{
    class MineSweeper
    {
        #region Variables Constructor and Properties

        public enum GameStateEnum { Default, Victory, Defeat }

        public enum MapEnum { Sourrounding0, Sourrounding1, Sourrounding2, Sourrounding3, Sourrounding4, Sourrounding5, Sourrounding6, Mine, Covered, BackgroundPlaceholder, Flag, NewGame, BackgroundDefeat, BackgroundVictory, BackgroundDefault, Counter, Shift }

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
        Random r = new Random();
        //Variabled for generating mines
        public GameStateEnum GameState { get; private set; } = GameStateEnum.Default;

        //Variabled for generating mines
        public static bool KeyboardDisplayShown { get; set; } = false;

        //Variabled for generating mines
        int[] availeableBombField;
        int availeableBombFieldCounter;

        //How many fields are covered
        int covered;
        //How many fields are flagged
        int flagged;
        
        public byte[,] Colors { get; set; } = new byte[17,3];

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
                if (value)
                {
                    display[0, 4] = (int)MapEnum.BackgroundPlaceholder;
                    display[13, 4] = (int)MapEnum.BackgroundPlaceholder;
                }
                else
                {
                    display[0, 4] = (int)MapEnum.Shift;
                    display[13, 4] = (int)MapEnum.Shift;
                }
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

                //add to total game counter
                Total++;

                keyboardLayout.SaveFile.IncrementTotal(Settings.Bombs);

                //start timer on first move
                UpdateStatsEvent();
                StartWatchEvent();

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
            else if (display[(i % 12) + 1, (i / 12) + 1] == (int)MapEnum.Flag)
            {
                display[(i % 12) + 1, (i / 12) + 1] = (int)MapEnum.Covered;
                isFlag[(i % 12) + 1, (i / 12) + 1] = false;
                flagged--;
                PrintLogiLED();
            }
            //place flag if field is empty
            else if (display[(i % 12) + 1, (i / 12) + 1] == (int)MapEnum.Covered)
            {
                display[(i % 12) + 1, (i / 12) + 1] = (int)MapEnum.Flag;
                isFlag[(i % 12) + 1, (i / 12) + 1] = true;
                flagged++;
                PrintLogiLED();
            }
        }

        #endregion

        #region New Game and Bomb Generation

        public void NewGame()
        {
            GameState = GameStateEnum.Default;

            //so you cant start right after new game
            App.last = 107;

            ResetDisplay();

            covered = keyboardLayout.CoveredFields;

            //so timer can be started when key i spressed and firstmove is true
            firstMove = true;
            isFlag = new bool[14, 6];
            flagged = 0;

            gameRunning = true;

            PrintLogiLED(true, true);
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
            for (int i = 0; i < display.GetLength(0); i++)
            {
                for (int j = 0; j < display.GetLength(1); j++)
                {
                    if (i > 0 && i < 13 && j > 0 && j < 5)
                    {
                        if (!keyboardLayout.EnabledKeys[j - 1, i - 1])
                        {
                            display[i, j] = (int)MapEnum.BackgroundPlaceholder;
                        }
                        else
                        {
                            display[i, j] = (int)MapEnum.Covered;
                        }
                    }
                    else
                    {
                        display[i, j] = (int)MapEnum.BackgroundPlaceholder;
                    }
                }
            }

            //num keys
            display[18, 5] = (int)MapEnum.Sourrounding0;
            display[17, 4] = (int)MapEnum.Sourrounding1;
            display[18, 4] = (int)MapEnum.Sourrounding2;
            display[19, 4] = (int)MapEnum.Sourrounding3;
            display[17, 3] = (int)MapEnum.Sourrounding4;
            display[18, 3] = (int)MapEnum.Sourrounding5;
            display[19, 3] = (int)MapEnum.Sourrounding6;

            //shiftkeys
            if (UseBackground)
            {
                display[0, 4] = (int)MapEnum.BackgroundPlaceholder;
                display[13, 4] = (int)MapEnum.BackgroundPlaceholder;
            }
            else
            {
                display[0, 4] = (int)MapEnum.Shift;
                display[13, 4] = (int)MapEnum.Shift;
            }

            //new game
            display[20, 2] = (int)MapEnum.NewGame;
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
                        continue;
                    }
                    if (isBomb[i + 1, j + 1]) map[i, j] = (int)MapEnum.Mine;
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
            PrintLogiLED(true, false);
        }

        public void PrintLogiLED(bool printDisplay, bool setBackground)
        {
            //init bitmap that will be used to create light
            byte[] logiLED = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
            
            UpdateBackground();
            
            //bomb counter
            if (GameState == GameStateEnum.Default)
            {
                int counter = Settings.Bombs - flagged;
                if (counter > 12) counter = 12;
                else if (counter < 0) counter = 0;
                
                for (int i = 0; i < counter; i++)
                {
                    display[1 + i, 0] = (int)MapEnum.Counter;
                }

                display[counter + 1, 0] = (int)MapEnum.BackgroundPlaceholder;
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    display[i, 0] = (int)MapEnum.BackgroundPlaceholder;
                }
            }

            //only print the in-app keyboard when the tab is selected
            if (MineSweeper.KeyboardDisplayShown && printDisplay)
            {
                PrintEvent();
            }
            
            //for actually printing the board
            for (int i = 0; i < display.GetLength(1); i++)
            {
                for (int j = 0; j < display.GetLength(0); j++)
                {
                    ColorToByte(logiLED, (i * 21 + j) * 4, Colors[display[j, i], 0], Colors[display[j, i], 1], Colors[display[j, i], 2]);
                }
            }

            //disabled, can be enabled by setting setlogilogo in config.cs to true
            //bool trigger for setting background as it would shortly flash if set every time
            if (Config.SetLogiLogo && setBackground)
            {
                setBackground = false;
                LogitechGSDK.LogiLedSetLighting(Convert.ToInt32((Convert.ToDouble(Colors[(int)MapEnum.BackgroundPlaceholder, 2]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(Colors[(int)MapEnum.BackgroundPlaceholder, 1]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(Colors[(int)MapEnum.BackgroundPlaceholder, 0]) / 255.0) * 100));
            }

            Debug.WriteLine(PrintBombs());

            //display the new color
            LogitechGSDK.LogiLedSetLightingFromBitmap(logiLED);
        }

        private void UpdateBackground()
        {
            switch (GameState)
            {
                case GameStateEnum.Default:
                    Colors[(int)MapEnum.BackgroundPlaceholder, 0] = Colors[(int)MapEnum.BackgroundDefault, 0];
                    Colors[(int)MapEnum.BackgroundPlaceholder, 1] = Colors[(int)MapEnum.BackgroundDefault, 1];
                    Colors[(int)MapEnum.BackgroundPlaceholder, 2] = Colors[(int)MapEnum.BackgroundDefault, 2];
                    break;
                case GameStateEnum.Victory:
                    Colors[(int)MapEnum.BackgroundPlaceholder, 0] = Colors[(int)MapEnum.BackgroundVictory, 0];
                    Colors[(int)MapEnum.BackgroundPlaceholder, 1] = Colors[(int)MapEnum.BackgroundVictory, 1];
                    Colors[(int)MapEnum.BackgroundPlaceholder, 2] = Colors[(int)MapEnum.BackgroundVictory, 2];
                    break;
                case GameStateEnum.Defeat:
                    Colors[(int)MapEnum.BackgroundPlaceholder, 0] = Colors[(int)MapEnum.BackgroundDefeat, 0];
                    Colors[(int)MapEnum.BackgroundPlaceholder, 1] = Colors[(int)MapEnum.BackgroundDefeat, 1];
                    Colors[(int)MapEnum.BackgroundPlaceholder, 2] = Colors[(int)MapEnum.BackgroundDefeat, 2];
                    break;
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
            //return if already ucovered
            if (display[x + 1, y + 1] != (int)MapEnum.Covered) return;

            //set m to value of the bomb map
            int m = map[x, y];
            
            if (m <= (int)MapEnum.Sourrounding6 && m >= (int)MapEnum.Sourrounding0)
            {
                display[x + 1, y + 1] = m;

                App.last = -1;

                if (--covered <= Settings.Bombs)
                {
                    Victory();
                    return;
                }
            }
            else if (m == (int)MapEnum.Mine)
            {
                Defeat();
                return;
            }

            //if empty recursively call funtion from all surrounding fields
            if (m == (int)MapEnum.Sourrounding0)
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

            //uncover all mines
            for (int i = 0; i < isBomb.GetLength(0); i++)
            {
                for (int j = 0; j < isBomb.GetLength(1); j++)
                {
                    if (isBomb[i, j]) display[i, j] = (int)MapEnum.Mine;
                }
            }
            
            PrintLogiLED(true, true);

            gameRunning = false;
        }

        private void Victory()
        {
            Wins++;

            keyboardLayout.SaveFile.IncrementWins(Settings.Bombs);

            StopWatchVictoryEvent();

            GameState = GameStateEnum.Victory;

            GameOver();
        }

        private void Defeat()
        {
            Losses++;

            keyboardLayout.SaveFile.IncrementLosses(Settings.Bombs);

            StopWatchDefeatEvent();

            GameState = GameStateEnum.Defeat;

            GameOver();
        }

        #endregion
    }
}
