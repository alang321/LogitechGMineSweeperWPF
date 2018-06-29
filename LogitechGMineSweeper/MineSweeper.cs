using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LogitechGMineSweeper
{
    public class MineSweeper
    {
        #region Enums

        public enum TimerStateEnum { started, resetButNotStarted, stoppedDefeat, stoppedVictory, stoppedNewRecord }

        public enum GameStateEnum { Default, Victory, Defeat }

        public enum MapEnum { Sourrounding0, Sourrounding1, Sourrounding2, Sourrounding3, Sourrounding4, Sourrounding5, Sourrounding6, Mine, Covered, BackgroundPlaceholder, Flag, NewGame, BackgroundDefeat, BackgroundVictory, BackgroundDefault, Counter, Shift }

        #endregion

        #region Events

        public delegate void PrintdisplayEventHandler();
        public delegate void UpdateStatsEventHandler();
        public delegate void TimerEventHandler(UpdateTimerEventArgs TimerState);
        public delegate void KeyboardLayoutChangedEventHandler(KeyboardLayoutChangedEventArgs Index);

        public static event KeyboardLayoutChangedEventHandler KeyLayoutChangedEvent;
        public static event PrintdisplayEventHandler PrintEvent;
        public static event UpdateStatsEventHandler StatsChangedEvent;
        //event called when timer state changes, when its started for example, event arg contains int with a value corresponding to a state
        public static event TimerEventHandler UpdateTimerEvent;

        #endregion

        #region Variables

        //Save Files
        public SaveFileSettings Settings { get; set; }
        public SaveFileGlobalStatistics GlobalStats { get; set; }
        public SaveFileColors ColorsFile { get; set; }

        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        
        KeyboardLayout keyboardLayout;

        //contains the number of sourrounding mines for every field
        int[,] map;
        //whether a field is a mine
        bool[,] isBomb = new bool[14, 6];
        //whether a field is a flag
        bool[,] isFlag = new bool[14, 6];
        //Contains what is displayed, display field is set to map field value on keypress
        public int[,] Display { get; private set; }
        //bool whether game is running, if false newgame() is called on keypress
        bool gameRunning;
        //bool if it is first move, if key pressed with first move true bombs are generated and total game number is incremented
        bool firstMove = true;
        //contains gamnestate, used for background color
        public GameStateEnum GameState { get; private set; } = GameStateEnum.Default;
        //How many fields are covered
        int covered;
        //key id of last pressed key
        int last = -1;

        //Variabled for generating mines
        Random r = new Random();

        //How many fields are flagged
        public int Flagged { get; private set; }
        //array with corresponding color to display value, 9 is placeholder for background color
        public byte[,] Colors { get; set; } = new byte[17,3];

        #endregion

        #region Constructor and Destructor

        public MineSweeper(SaveFileSettings settings, SaveFileGlobalStatistics globalStats, KeyboardLayout keyLayout, SaveFileColors ColorsFile)
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
            this.ColorsFile = ColorsFile;
            this.Colors = ColorsFile.SavedColors;
            this.Settings = settings;
            this.GlobalStats = globalStats;
            this.KeyboardLayout = keyLayout;
        }

        ~MineSweeper()
        {
            UnhookWindowsHookEx(_hookID);
        }

        #endregion

        #region Properties

        public int Bombs
        {
            get { return Settings.Bombs; }
            set
            {
                if(Settings.Bombs != value)
                {
                    Settings.Bombs = value;
                    NewGame();
                }
            }
        }

        public bool SetLogiLogo
        {
            get { return Settings.SetLogiLogo; }
            set
            {
                if (Settings.SetLogiLogo != value)
                {
                    Settings.SetLogiLogo = value;
                    PrintLogiLED(true);
                }
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
                    Display[0, 4] = (int)MapEnum.BackgroundPlaceholder;
                    Display[13, 4] = (int)MapEnum.BackgroundPlaceholder;
                }
                else
                {
                    Display[0, 4] = (int)MapEnum.Shift;
                    Display[13, 4] = (int)MapEnum.Shift;
                }
                Settings.UseBackground = value;
            }
        }

        public KeyboardLayout KeyboardLayout
        {
            get { return keyboardLayout; }
            set
            {
                try
                {
                    if (keyboardLayout.Index != value.Index)
                    {
                        keyboardLayout = value;
                        Settings.LayoutIndex = keyboardLayout.Index;
                        KeyLayoutChangedEvent?.Invoke(new KeyboardLayoutChangedEventArgs(keyboardLayout.Index));
                        NewGame();
                    }
                }
                catch
                {
                    KeyLayoutChangedEvent?.Invoke(new KeyboardLayoutChangedEventArgs(keyboardLayout.Index));
                    keyboardLayout = value;
                    Settings.LayoutIndex = keyboardLayout.Index;
                    NewGame();
                }
            }
        }

        #endregion

        #region Get Keypress

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Debug.WriteLine("Key ID-Code: " + vkCode);
                if (keyboardLayout.KeyIds.Contains(vkCode))
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        if (last != 107 && vkCode == 107) KeyPressed(48);
                        else if (vkCode != 107)
                        {
                            SetFlag(Array.IndexOf(keyboardLayout.KeyIds, vkCode));
                            last = -1;
                        }
                    }
                    else if (last != vkCode)
                    {
                        last = vkCode;
                        KeyPressed(Array.IndexOf(keyboardLayout.KeyIds, vkCode));
                    }
                    else
                    {
                        Debug.WriteLine("REJECTED: Double Press - Key ID-Code: " + vkCode);
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        #endregion

        #region Key Press Handling

        //Handler for Key presses, gest passed number corresponding to field, from the intercept keys class
        public void KeyPressed(int i)
        {
            //Start Game If not Running
            if (!gameRunning || i == 48)
            {
                NewGame();
            }
            //if field is already uncovered checkl if right amout of flags sourrounds it
            else if (Display[(i % 12) + 1, (i / 12) + 1] <= 6 && Display[(i % 12) + 1, (i / 12) + 1] >= 0)
            {
                UncoverFlags(i % 12, i / 12);
            }
            else
            {
                Uncover(i % 12, i / 12);
                PrintLogiLED();
            }
        }

        public void SetFlag(int i)
        {
            //Start Game If not Running
            if (!gameRunning || i == 48)
            {
                NewGame();
            }
            //take away flag if already present
            else if (Display[(i % 12) + 1, (i / 12) + 1] == (int)MapEnum.Flag)
            {
                Display[(i % 12) + 1, (i / 12) + 1] = (int)MapEnum.Covered;
                isFlag[(i % 12) + 1, (i / 12) + 1] = false;
                Flagged--;
                PrintLogiLED();
            }
            //place flag if field is empty
            else if (Display[(i % 12) + 1, (i / 12) + 1] == (int)MapEnum.Covered)
            {
                Display[(i % 12) + 1, (i / 12) + 1] = (int)MapEnum.Flag;
                isFlag[(i % 12) + 1, (i / 12) + 1] = true;
                Flagged++;
                PrintLogiLED();
            }
        }

        #endregion

        #region New Game and Bomb Generation

        public void NewGame()
        {
            GameState = GameStateEnum.Default;

            Stopwatch.Reset();

            UpdateTimerEvent?.Invoke(new UpdateTimerEventArgs((int)TimerStateEnum.resetButNotStarted));

            //so you cant start right after new game
            last = 107;

            ResetDisplay();

            covered = keyboardLayout.CoveredFields;

            //so timer can be started when key is pressed and firstmove is true
            firstMove = true;
            isFlag = new bool[14, 6];
            Flagged = 0;

            gameRunning = true;

            PrintLogiLED(true);
        }

        private void GenBombs(int x, int y)
        {
            isBomb = new bool[14, 6];
            int[] availeableBombField = new int[48];
            int availeableBombFieldCounter = 0;
            
            for (int i = 0; i < keyboardLayout.EnabledKeys.GetLength(0); i++)
            {
                for (int j = 0; j < keyboardLayout.EnabledKeys.GetLength(1); j++)
                {
                    if ((i != x || j != y) && keyboardLayout.EnabledKeys[i, j]) availeableBombField[availeableBombFieldCounter++] = i * keyboardLayout.EnabledKeys.GetLength(1) + j;
                }
            }

            for(int i = 0; i < Bombs; i++)
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
            Display = new int[21, 6];
            for (int i = 0; i < Display.GetLength(0); i++)
            {
                for (int j = 0; j < Display.GetLength(1); j++)
                {
                    if (i > 0 && i < 13 && j > 0 && j < 5)
                    {
                        if (!keyboardLayout.EnabledKeys[j - 1, i - 1])
                        {
                            Display[i, j] = (int)MapEnum.BackgroundPlaceholder;
                        }
                        else
                        {
                            Display[i, j] = (int)MapEnum.Covered;
                        }
                    }
                    else
                    {
                        Display[i, j] = (int)MapEnum.BackgroundPlaceholder;
                    }
                }
            }

            //num keys
            Display[18, 5] = (int)MapEnum.Sourrounding0;
            Display[17, 4] = (int)MapEnum.Sourrounding1;
            Display[18, 4] = (int)MapEnum.Sourrounding2;
            Display[19, 4] = (int)MapEnum.Sourrounding3;
            Display[17, 3] = (int)MapEnum.Sourrounding4;
            Display[18, 3] = (int)MapEnum.Sourrounding5;
            Display[19, 3] = (int)MapEnum.Sourrounding6;

            //shiftkeys
            if (UseBackground)
            {
                Display[0, 4] = (int)MapEnum.BackgroundPlaceholder;
                Display[13, 4] = (int)MapEnum.BackgroundPlaceholder;
            }
            else
            {
                Display[0, 4] = (int)MapEnum.Shift;
                Display[13, 4] = (int)MapEnum.Shift;
            }

            //new game
            Display[20, 2] = (int)MapEnum.NewGame;
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
            for (int i = 0; i < Display.GetLength(1); i++)
            {
                for (int j = 0; j < Display.GetLength(0); j++)
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
                    if (Display[j, i] == 7) s += "X ";
                    else if (Display[j, i] == 8) s += "- ";
                    else s += Display[j, i] + " ";
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
            PrintLogiLED(false);
        }

        public void PrintLogiLED(bool setBackground)
        {
            //init bitmap that will be used to create light
            byte[] logiLED = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
            
            UpdateBackground();
            
            //bomb counter
            if (GameState == GameStateEnum.Default)
            {
                int counter = Bombs - Flagged;
                if (counter > 12) counter = 12;
                else if (counter < 0) counter = 0;
                
                for (int i = 0; i < counter; i++)
                {
                    Display[1 + i, 0] = (int)MapEnum.Counter;
                }

                Display[counter + 1, 0] = (int)MapEnum.BackgroundPlaceholder;
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    Display[i, 0] = (int)MapEnum.BackgroundPlaceholder;
                }
            }
            
            //for actually printing the board
            for (int i = 0; i < Display.GetLength(1); i++)
            {
                for (int j = 0; j < Display.GetLength(0); j++)
                {
                    ColorToByte(logiLED, (i * 21 + j) * 4, Colors[Display[j, i], 0], Colors[Display[j, i], 1], Colors[Display[j, i], 2]);
                }
            }
            
            //bool trigger for setting background as it would shortly flash if set every time
            if (SetLogiLogo && setBackground)
            {
                LogitechGSDK.LogiLedSetLighting(Convert.ToInt32((Convert.ToDouble(Colors[(int)MapEnum.BackgroundPlaceholder, 2]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(Colors[(int)MapEnum.BackgroundPlaceholder, 1]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(Colors[(int)MapEnum.BackgroundPlaceholder, 0]) / 255.0) * 100));
            }

            Debug.WriteLine(PrintBombs());

            //raise print event
            PrintEvent?.Invoke();

            //Display the new color
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

        #region Uncover Field

        private void Uncover(int x, int y)
        {
            if (Display[x + 1, y + 1] != (int)MapEnum.Covered) return;

            //generate bomb on first move
            if (firstMove)
            {
                GenBombs(y, x);

                //add to total game counter
                Total++;
                keyboardLayout.SaveFile.IncrementTotal(Bombs);
                StatsChangedEvent?.Invoke();

                Stopwatch.Start();
                UpdateTimerEvent?.Invoke(new UpdateTimerEventArgs((int)TimerStateEnum.started));

                firstMove = false;
            }

            //set m to value of the bomb map
            int m = map[x, y];

            if (m <= (int)MapEnum.Sourrounding6 && m >= (int)MapEnum.Sourrounding0)
            {
                Display[x + 1, y + 1] = m;

                last = -1;

                if (--covered <= Bombs)
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

            if (Display[x + 1, y + 1] <= sourroundingFlags)
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
            StatsChangedEvent?.Invoke();

            //so you cant spam new game
            last = -1;

            //uncover all mines
            for (int i = 0; i < isBomb.GetLength(0); i++)
            {
                for (int j = 0; j < isBomb.GetLength(1); j++)
                {
                    if (isBomb[i, j]) Display[i, j] = (int)MapEnum.Mine;
                }
            }
            
            PrintLogiLED(true);

            gameRunning = false;
        }

        private void Victory()
        {
            Wins++;
            keyboardLayout.SaveFile.IncrementWins(Bombs);

            Stopwatch.Stop();
            
            int bestTime = keyboardLayout.SaveFile.GetBestTime(Bombs);
            if (bestTime == -1 || bestTime > Stopwatch.Elapsed.TotalMilliseconds)
            {
                keyboardLayout.SaveFile.UpdateBestTime(Bombs, Convert.ToInt32(Stopwatch.Elapsed.TotalMilliseconds));
                UpdateTimerEvent?.Invoke(new UpdateTimerEventArgs((int)TimerStateEnum.stoppedNewRecord));
            }
            else UpdateTimerEvent?.Invoke(new UpdateTimerEventArgs((int)TimerStateEnum.stoppedVictory));
            
            GameState = GameStateEnum.Victory;

            GameOver();
        }

        private void Defeat()
        {
            Losses++;
            keyboardLayout.SaveFile.IncrementLosses(Bombs);

            Stopwatch.Stop();
            UpdateTimerEvent?.Invoke(new UpdateTimerEventArgs((int)TimerStateEnum.stoppedDefeat));

            GameState = GameStateEnum.Defeat;

            GameOver();
        }

        #endregion
    }

    #region Event Args

    public class UpdateTimerEventArgs : EventArgs
    {
        public UpdateTimerEventArgs(int state)
        {
            this.State = state;
        }

        public int State { get; private set; }
    }

    public class KeyboardLayoutChangedEventArgs : EventArgs
    {
        public KeyboardLayoutChangedEventArgs(int index)
        {
            this.Index = index;
        }

        public int Index { get; private set; }
    }

    #endregion
}
