using System;
using System.IO;
using System.Windows.Media;

namespace LogitechGMineSweeper
{
    public class Config
    {
        #region settings default values

        public static int BombsDefault { get; } = 14;
        public static int KeyboardLayoutDefaultIndex { get; } = (int)Config.Layout.DE;

        //whether the shift keys use the background color or have a spoecific one
        public static bool UseBackgroundDefault { get; } = true;

        //Set Logitech Logo to background color makes screen flash so not used
        public static bool defaultSetLogiLogo  = false;

        public static byte[,] ColorsDefault { get; } = { {000,000,000}, {255,000,000}, {255,255,000}, {000,128,000}, {000,255,255}, {000,127,255}, {128,000,128}, {000,000,255}, {255,255,255}, {255,200,200}, {255,000,255}, {255,000,000}, {000,000,255}, {000,255,255}, {255,160,160}, {000,255,255}, {255,000,255} };
        
        #endregion

        #region save file paths

        public static string SystemPath { get; } = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string Directory { get; } = Path.Combine(SystemPath, "Logitech MineSweeper");

        public static string PathColorsFile = Path.Combine(Directory, "colors.txt");
        public static string PathSettingsFile = Path.Combine(Directory, "settings.txt");
        public static string PathGlobalStatisticsFile = Path.Combine(Directory, "statsTotal.txt");

        #endregion

        #region timer

        //max timer duration
        public static TimeSpan MaxTimerValue { get; } = TimeSpan.FromSeconds(3599);

        //atrinf´g display when time is not
        public static string TimeNotSetText { get; } = "--:--";

        //Timer colors
        public static Color Defeat { get; set;  } = Colors.Red;
        public static Color Victory { get; set; } = Colors.Green;
        public static Color NewRecord { get; set; } = Colors.Green;
        public static Color Default { get; set; } = Colors.Black;

        //text that is displayed next to timer on new record
        public static string TextNewRecord { get; } = "!";

        #endregion

        #region Animations
        
        public static TimeSpan ArrowAnimDuration { get; } = TimeSpan.FromSeconds(0.1);
        public static TimeSpan HamburgerAnimDuration { get; } = TimeSpan.FromSeconds(0.1);
        public static double ArrowAcceleration { get; } = 0.5;
        public static double ArrowDeceleration { get; } = 0.5;
        public static double HamburgerAcceleration { get; } = 0.5;
        public static double HamburgerDeceleration { get; } = 0.5;
        
        public static TimeSpan MenuCollapseExpandDuration { get; } = TimeSpan.FromSeconds(0.2);
        public static double MenuExpandWidth { get; } = 183;

        public static TimeSpan MenuCollapseTextSlideOutStart { get; } = MenuCollapseExpandDuration - TimeSpan.FromSeconds(0.05);

        //Nav Menu
        public static TimeSpan BlackSideBarSpeed { get; } = TimeSpan.FromSeconds(0.15);
        public static TimeSpan PagesSlideSpeed { get; } = TimeSpan.FromSeconds(0.2);

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
        //then add the keyids of the corresponding keys on the used layout, key ids are printed to console in debug mode
        //to disable key enter -1 in the key ids array
        //then add a keyboard display in folder keyboardlayouts in InitKeyboardLayoutsArray function like its been done before
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
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 219, 221,
                                81, 87, 69, 82, 84, 90, 85, 73, 79, 80, 186, 187,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 192, 222, 191,
                                226, 89, 88, 67, 86, 66, 78, 77, 188, 190, 189, -1, /*Add Key*/ 107 },
                    //new LogitechGMineSweeper.KeyboardLayouts.DE()
                    null
                ),
            //US
            new KeyboardLayout
                (
                    //statistics file
                    new SaveFileStatitics(Path.Combine(Directory, "US.txt")),
                    //text
                    "US",
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 189, 187,
                                81, 87, 69, 82, 84, 89, 85, 73, 79, 80, 219, 221,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 186, 222, -1,
                                -1, 90, 88, 67, 86, 66, 78, 77, 188, 190, 191, -1, /*Add Key*/ 107 },
                    //new LogitechGMineSweeper.KeyboardLayouts.US()
                    null
                ),
            //UK
            new KeyboardLayout
                (
                    //statistics file
                    new SaveFileStatitics(Path.Combine(Directory, "UK.txt")),
                    //text
                    "UK",
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 189, 187,
                                81, 87, 69, 82, 84, 89, 85, 73, 79, 80, 219, 221,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 186, 192, 222,
                                220, 90, 88, 67, 86, 66, 78, 77, 188, 190, 191, -1, /*Add Key*/ 107 },
                    //new LogitechGMineSweeper.KeyboardLayouts.UK()
                    null
                ),
            //ITA
            new KeyboardLayout
                (
                    //statistics file
                    new SaveFileStatitics(Path.Combine(Directory, "ITA.txt")),
                    //text
                    "IT",
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 219, 221,
                                81, 87, 69, 82, 84, 89, 85, 73, 79, 80, 186, 187,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 192, 222, 191,
                                226, 90, 88, 67, 86, 66, 78, 77, 188, 190, 189, -1, /*Add Key*/ 107 },
                    //new LogitechGMineSweeper.KeyboardLayouts.IT()
                    null
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

        public static bool MouseWheelForNavigation { get; } = false;

        //array for the words displayed in the color picker popup
        public static string[] ColorPickerTitles { get; } = { "0 Bombs", "1 Bomb", "2 Bombs", "3 Bombs", "4 Bombs", "5 Bombs", "6 Bombs", "Bomb Field", "Covered Field", "Offboard", "Flag", "New Game Key", "Defeat Background", "Victory Background", "Default Background", "Bomb Counter", "Shift Keys" };
    }
}
