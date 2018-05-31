using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LogitechGMineSweeper
{
    static class Config
    {
        public static string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string directory = Path.Combine(systemPath, "Logitech MineSweeper");

        public static string fileColors = Path.Combine(directory, "colors.txt");
        public static string fileConfig = Path.Combine(directory, "config.txt");

        //to add layout add everything in keyboardlayout class
        //the enabledKeys array controls which keys are enabled, here for example on the us keyboard the bottom left is disabled as it is not present on the keyboard so no bombs will be generated there
        //then add the keyids of the corresponding keys on the used layout, key ids are printed to console in debug mode
        //then add a keyboard display in folder keyboardlayouts in InitKeyboardLayoutsArray function like its been done beofre
        public enum Layout { DE, US, UK, IT }

        //called in constructor of mainwindow, exception thrown when implemented in array initialisation
        public static void InitKeyboardLayoutsArray()
        {
            KeyboardLayouts[0].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.DE();
            KeyboardLayouts[1].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.US();
            KeyboardLayouts[2].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.UK();
            KeyboardLayouts[3].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.IT();
        }

        //Keyboard Layout Objects
        public static KeyboardLayout[] KeyboardLayouts =
        {
            //DE
            new KeyboardLayout
                (
                    //statistics file
                    Path.Combine(directory, "DE.txt"),
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
                    //new KeyboardLayouts.DE()
                    null
                ),
            //US
            new KeyboardLayout
                (
                    //statistics file
                    Path.Combine(directory, "US.txt"),
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
                    //new KeyboardLayouts.US()
                    null
                ),
            //UK
            new KeyboardLayout
                (
                    //statistics file
                    Path.Combine(directory, "UK.txt"),
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
                    //new KeyboardLayouts.UK()
                    null
                ),
            //ITA
            new KeyboardLayout
                (
                    //statistics file
                    Path.Combine(directory, "ITA.txt"),
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
                    //new KeyboardLayouts.UK()
                    null
                ),
        };

        public static int bombsDefault = 14;
        public static int keyboardLayoutDefault = (int)Config.Layout.DE;


        public static string[] statisticsDefault = { "0: .01800000.10.20.30.4", "1: .01800000.10.20.30.4", "2: .01800000.10.20.30.4", "3: .01800000.10.20.30.4", "4: .01800000.10.20.30.4", "5: .01800000.10.20.30.4", "6: .01800000.10.20.30.4", "7: .01800000.10.20.30.4", "8: .01800000.10.20.30.4", "9: .01800000.10.20.30.4", "10: .01800000.10.20.30.4", "11: .01800000.10.20.30.4", "12: .01800000.10.20.30.4", "13: .01800000.10.20.30.4", "14: .01800000.10.20.30.4", "15: .01800000.10.20.30.4", "16: .01800000.10.20.30.4", "17: .01800000.10.20.30.4", "18: .01800000.10.20.30.4", "19: .01800000.10.20.30.4", "20: .01800000.10.20.30.4", "21: .01800000.10.20.30.4", "22: .01800000.10.20.30.4", "23: .01800000.10.20.30.4", "24: .01800000.10.20.30.4", "25: .01800000.10.20.30.4", "26: .01800000.10.20.30.4", "27: .01800000.10.20.30.4", "28: .01800000.10.20.30.4", "29: .01800000.10.20.30.4", "30: .01800000.10.20.30.4", "31: .01800000.10.20.30.4", "32: .01800000.10.20.30.4", "33: .01800000.10.20.30.4", "34: .01800000.10.20.30.4", "35: .01800000.10.20.30.4", "36: .01800000.10.20.30.4", "37: .01800000.10.20.30.4", "38: .01800000.10.20.30.4", "39: .01800000.10.20.30.4", "40: .01800000.10.20.30.4", "41: .01800000.10.20.30.4", "42: .01800000.10.20.30.4", "43: .01800000.10.20.30.4", "44: .01800000.10.20.30.4", "45: .01800000.10.20.30.4", "46: .01800000.10.20.30.4", "47: .01800000.10.20.30.4", "48: .01800000.10.20.30.4" };
        public static string[] colorsDefault = { "000,000,000", "255,000,000", "255,255,000", "000,128,000", "000,255,255", "000,127,255", "128,000,128", "000,000,255", "255,255,255", "255,200,200", "255,000,255", "255,000,000", "000,000,255", "000,255,255", "255,160,160", "000,255,255", "255,000,255" };
        public static string[] configDefault = { "Wins: 0", "Bombs: " + bombsDefault.ToString(), "Layout: " + keyboardLayoutDefault, "Total: 0", "Losses: 0", "UseBackground: " + useBackgroundDefault };

        //whether the shift keys use the background color or have a spoecific one
        public static bool useBackgroundDefault = false;

        public static int hard = 14;
        public static int medium = 10;
        public static int easy = 7;
        
        //values between 0-44 and maxbombs bigger than minbombs
        public static int maxBombs = 44;
        public static int minBombs = 0;

        public static int NumUDstartvalue = bombsDefault;

        //Set Logitech Logo to background color makes screen flash so not used
        public static bool setLogiLogo = false;

        //array for the words displayed in the color picker popup
        public static string[] colorPickerTitles = { "0 Bombs", "1 Bomb", "2 Bombs", "3 Bombs", "4 Bombs", "5 Bombs", "6 Bombs", "Bomb Field", "Covered Field", "Offboard", "Flag", "New Game Key", "Defeat Background", "Victory Background", "Default Background", "Bomb Counter", "Shift Keys" };

        //Threshold for when the foreground color is white and black, when total rgb color is below this value foreground color is white else its black
        public static int foregroundThreshold = 270;

        //Timer colors
        public static Color Defeat = Colors.Red;
        public static Color Victory = Colors.Green;
        public static Color NewRecord = Colors.Green;
        public static Color Default = Colors.Black;

        //text that is displayed next to timer on new record
        public static string textNewRecord = " - New Best!";
    }
}

