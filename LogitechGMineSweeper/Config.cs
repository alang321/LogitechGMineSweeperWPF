using System;
using System.IO;
using System.Windows.Media;

namespace LogitechGMineSweeper
{
    class Config
    {
        #region settings default values

        public static int BombsDefault { get; } = 14;
        public static int KeyboardLayoutDefaultIndex { get; } = (int)Config.Layout.DE;
        
        //whether the shift keys use the background color or have a spoecific one
        public static bool UseBackgroundDefault { get; } = true;

        #endregion

        #region save file paths & default values

        public static string SystemPath { get; } = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string Directory { get; } = Path.Combine(SystemPath, "Logitech MineSweeper");
        
        public static string PathColorsFile = Path.Combine(Directory, "colors.txt");
        public static string PathSettingsFile = Path.Combine(Directory, "settings.txt");
        public static string PathGlobalStatisticsFile = Path.Combine(Directory, "statsTotal.txt");

        //default save file 
        public static string[] StatisticsDefault { get; } = { "0: .0-1.10.20.30.4", "1: .0-1.10.20.30.4", "2: .0-1.10.20.30.4", "3: .0-1.10.20.30.4", "4: .0-1.10.20.30.4", "5: .0-1.10.20.30.4", "6: .0-1.10.20.30.4", "7: .0-1.10.20.30.4", "8: .0-1.10.20.30.4", "9: .0-1.10.20.30.4", "10: .0-1.10.20.30.4", "11: .0-1.10.20.30.4", "12: .0-1.10.20.30.4", "13: .0-1.10.20.30.4", "14: .0-1.10.20.30.4", "15: .0-1.10.20.30.4", "16: .0-1.10.20.30.4", "17: .0-1.10.20.30.4", "18: .0-1.10.20.30.4", "19: .0-1.10.20.30.4", "20: .0-1.10.20.30.4", "21: .0-1.10.20.30.4", "22: .0-1.10.20.30.4", "23: .0-1.10.20.30.4", "24: .0-1.10.20.30.4", "25: .0-1.10.20.30.4", "26: .0-1.10.20.30.4", "27: .0-1.10.20.30.4", "28: .0-1.10.20.30.4", "29: .0-1.10.20.30.4", "30: .0-1.10.20.30.4", "31: .0-1.10.20.30.4", "32: .0-1.10.20.30.4", "33: .0-1.10.20.30.4", "34: .0-1.10.20.30.4", "35: .0-1.10.20.30.4", "36: .0-1.10.20.30.4", "37: .0-1.10.20.30.4", "38: .0-1.10.20.30.4", "39: .0-1.10.20.30.4", "40: .0-1.10.20.30.4", "41: .0-1.10.20.30.4", "42: .0-1.10.20.30.4", "43: .0-1.10.20.30.4", "44: .0-1.10.20.30.4", "45: .0-1.10.20.30.4", "46: .0-1.10.20.30.4", "47: .0-1.10.20.30.4", "48: .0-1.10.20.30.4", Version };
        public static string[] ColorsDefault { get; } = { "000,000,000", "255,000,000", "255,255,000", "000,128,000", "000,255,255", "000,127,255", "128,000,128", "000,000,255", "255,255,255", "255,200,200", "255,000,255", "255,000,000", "000,000,255", "000,255,255", "255,160,160", "000,255,255", "255,000,255" };
        public static string[] SettingsDefault { get; } = { "Bombs: " + BombsDefault.ToString(), "Layout: " + KeyboardLayoutDefaultIndex, "UseBackground: " + UseBackgroundDefault };
        public static string[] GlobalDefault { get; } = { "Wins: 0",  "Losses: 0", "Total: 0" };

        #endregion

        #region timer

        //max timer duration in ms, 1 minute = 60000ms; 3600000ms = 1hour
        public static int MaxTimerValue { get; } = 3600000;

        //atrinf´g display when time is not
        public static string TimeNotSetText { get; } = "--:--";

        //Timer colors
        public static Color Defeat { get; set;  } = Colors.Red;
        public static Color Victory { get; set; } = Colors.Green;
        public static Color NewRecord { get; set; } = Colors.Green;
        public static Color Default { get; set; } = Colors.Black;

        //text that is displayed next to timer on new record
        public static string TextNewRecord { get; } = " - Record!";

        #endregion

        #region Bomb count values

        //values between 0-44 and maxbombs bigger than minbombs
        public static int MaxBombs { get; } = 40;
        public static int MinBombs { get; } = 3;

        public static int NumUDstartvalue { get; } = BombsDefault;

        #endregion

        #region TextColor 

        //for which colors foreground is important everything but background colors because they dont have text over them, if not in this array color is set to transparent
        public static int[] ForegroundColorImportant { get; } = { (int)MineSweeper.MapEnum.Sourrounding0, (int)MineSweeper.MapEnum.Sourrounding1, (int)MineSweeper.MapEnum.Sourrounding2, (int)MineSweeper.MapEnum.Sourrounding3, (int)MineSweeper.MapEnum.Sourrounding4, (int)MineSweeper.MapEnum.Sourrounding5, (int)MineSweeper.MapEnum.Sourrounding6, (int)MineSweeper.MapEnum.Mine, (int)MineSweeper.MapEnum.Covered, (int)MineSweeper.MapEnum.Flag, (int)MineSweeper.MapEnum.NewGame, (int)MineSweeper.MapEnum.Counter, (int)MineSweeper.MapEnum.Shift};

        //Threshold for when the foreground color is white and black, when total rgb color is below this value foreground color is white else its black
        public static int ForegroundThreshold { get; set; } = 270;

        #endregion

        #region keyboard layouts array

        //to add layout add everything in keyboardlayout class
        //the enabledKeys array controls which keys are enabled, here for example on the us keyboard the bottom left is disabled as it is not present on the keyboard so no bombs will be generated there
        //then add the keyids of the corresponding keys on the used layout, key ids are printed to console in debug mode
        //then add a keyboard display in folder keyboardlayouts in InitKeyboardLayoutsArray function like its been done beofre
        //add save file in init keyboardlayout array
        public enum Layout { DE, US, UK, IT }

        //Keyboard Layout Objects
        public static KeyboardLayout[] KeyboardLayouts { get; } =
        {
            //DE
            new KeyboardLayout
                (
                    //statistics file
                    new SaveFileStatitics(Path.Combine(Directory, "DE.txt")),
                    //text
                    "DE",
                    //present/enabled keys
                    new bool[,]{{ true, true, true, true, true, true, true, true, true, true, true, true },
                                 { true, true, true, true, true, true, true, true, true, true, true, true },
                                  { true, true, true, true, true, true, true, true, true, true, true, true },
                                 { true, true, true, true, true, true, true, true, true, true, true, false }},
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 219, 221,
                                81, 87, 69, 82, 84, 90, 85, 73, 79, 80, 186, 187,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 192, 222, 191,
                                226, 89, 88, 67, 86, 66, 78, 77, 188, 190, 189, -1, /*Add Key*/ 107 },
                    //Keyboard Display Uri
                    //new LogitechGMineSweeper.KeyboardLayouts.DE(),
                    null,
                    //easy
                    7,
                    //medium
                    10,
                    //hard
                    14
                ),
            //US
            new KeyboardLayout
                (
                    //statistics file
                    new SaveFileStatitics(Path.Combine(Directory, "US.txt")),
                    //text
                    "US",
                    //present/enabled keys
                    new bool[,]{{ true, true, true, true, true, true, true, true, true, true, true, true },
                                 { true, true, true, true, true, true, true, true, true, true, true, true },
                                  { true, true, true, true, true, true, true, true, true, true, true,false },
                                 {false, true, true, true, true, true, true, true, true, true, true,false }},
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 189, 187,
                                81, 87, 69, 82, 84, 89, 85, 73, 79, 80, 219, 221,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 186, 222, -1,
                                -1, 90, 88, 67, 86, 66, 78, 77, 188, 190, 191, -1, /*Add Key*/ 107 },
                    //Keyboard Display Uri
                    //new LogitechGMineSweeper.KeyboardLayouts.US(),
                    null,
                    //easy
                    7,
                    //medium
                    10,
                    //hard
                    14
                ),
            //UK
            new KeyboardLayout
                (
                    //statistics file
                    new SaveFileStatitics(Path.Combine(Directory, "UK.txt")),
                    //text
                    "UK",
                    //present/enabled keys
                    new bool[,]{{ true, true, true, true, true, true, true, true, true, true, true, true },
                                 { true, true, true, true, true, true, true, true, true, true, true, true },
                                  { true, true, true, true, true, true, true, true, true, true, true, true },
                                 { true, true, true, true, true, true, true, true, true, true, true,false }},
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 189, 187,
                                81, 87, 69, 82, 84, 89, 85, 73, 79, 80, 219, 221,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 186, 192, 222,
                                220, 90, 88, 67, 86, 66, 78, 77, 188, 190, 191, -1, /*Add Key*/ 107 },
                    //Keyboard Display Uri
                    //new LogitechGMineSweeper.KeyboardLayouts.UK(),
                    null,
                    //easy
                    7,
                    //medium
                    10,
                    //hard
                    14
                ),
            //ITA
            new KeyboardLayout
                (
                    //statistics file
                    new SaveFileStatitics(Path.Combine(Directory, "ITA.txt")),
                    //text
                    "IT",
                    //present/enabled keys
                    new bool[,]{{ true, true, true, true, true, true, true, true, true, true, true, true },
                                 { true, true, true, true, true, true, true, true, true, true, true, true },
                                  { true, true, true, true, true, true, true, true, true, true, true, true },
                                 { true, true, true, true, true, true, true, true, true, true, true,false }},
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 219, 221,
                                81, 87, 69, 82, 84, 89, 85, 73, 79, 80, 186, 187,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 192, 222, 191,
                                226, 90, 88, 67, 86, 66, 78, 77, 188, 190, 189, -1, /*Add Key*/ 107 },
                    //Keyboard Display Uri
                    //new LogitechGMineSweeper.KeyboardLayouts.IT(),
                    null,
                    //easy
                    7,
                    //medium
                    10,
                    //hard
                    14
                )
        };

        //called in constructor of mainwindow, exception thrown when implemented in array initialisation
        public static void InitKeyboardLayoutsArray()
        {
            KeyboardLayouts[0].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.DE();
            KeyboardLayouts[1].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.US();
            KeyboardLayouts[2].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.UK();
            KeyboardLayouts[3].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.IT();
        }

        #endregion

        public static MineSweeper MineSweeper { get; set; }

        public static string Version { get; } = "2.3.0";

        //Set Logitech Logo to background color makes screen flash so not used
        public static bool SetLogiLogo { get; set; } = false;

        //array for the words displayed in the color picker popup
        public static string[] ColorPickerTitles { get; } = { "0 Bombs", "1 Bomb", "2 Bombs", "3 Bombs", "4 Bombs", "5 Bombs", "6 Bombs", "Bomb Field", "Covered Field", "Offboard", "Flag", "New Game Key", "Defeat Background", "Victory Background", "Default Background", "Bomb Counter", "Shift Keys" };
    }
}
