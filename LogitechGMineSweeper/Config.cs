using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public enum Layout { DE, US, UK }

        //called in constructor of mainwindow, exception thrown when implemented in array initialisation
        public static void InitKeyboardLayoutsArray()
        {
            KeyboardLayouts[0].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.DE();
            KeyboardLayouts[1].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.US();
            KeyboardLayouts[2].KeyboardDisplayPage = new LogitechGMineSweeper.KeyboardLayouts.UK();
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
                                 { true, true, true, true, true, true, true, true, true, true, true,false }},
                    //the keyids
                    new int[]{ 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 219, 221,
                                81, 87, 69, 82, 84, 90, 85, 73, 79, 80, 186, 187,
                                 65, 83, 68, 70, 71, 72, 74, 75, 76, 192, 222, 191,
                                226, 89, 88, 67, 86, 66, 78, 77, 188, 190, 189, /*Add Key*/ 107 },
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
                                -1, 90, 88, 67, 86, 66, 78, 77, 188, 190, 191, /*Add Key*/ 107 },
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
                                220, 90, 88, 67, 86, 66, 78, 77, 188, 190, 191, /*Add Key*/ 107 },
                    //Keyboard Display Uri
                    //new KeyboardLayouts.UK()
                    null
                ),
        };

        public static int bombsDefault = 14;
        public static int keyboardLayoutDefault = (int)Config.Layout.DE;


        public static string[] statisticsDefault = { "", "1: 30:00", "2: 30:00", "3: 30:00", "4: 30:00", "5: 30:00", "6: 30:00", "7: 30:00", "8: 30:00", "9: 30:00", "10: 30:00", "11: 30:00", "12: 30:00", "13: 30:00", "14: 30:00", "15: 30:00", "16: 30:00", "17: 30:00", "18: 30:00", "19: 30:00", "20: 30:00", "21: 30:00", "22: 30:00", "23: 30:00", "24: 30:00", "25: 30:00", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        public static string[] colorsDefault = { "000,000,000", "255,000,000", "255,255,000", "000,128,000", "000,255,255", "000,127,255", "128,000,128", "000,000,255", "255,255,255", "255,200,200", "255,000,255", "255,000,000", "000,000,255", "000,255,255", "255,160,160", "000,255,255", "255,000,255" };
        public static string[] configDefault = { "Wins: 0", "Bombs: " + bombsDefault.ToString(), "Layout: " + keyboardLayoutDefault, "Total: 0", "Losses: 0", "UseBackground: " + useBackgroundDefault };

        //whether the shift keys use the background color or have a spoecific one
        public static bool useBackgroundDefault = false;

        public static int hard = 14;
        public static int medium = 10;
        public static int easy = 7;

        public static int maxBombs = 25;
        public static int minBombs = 5;

        public static int NumUDstartvalue = bombsDefault;

        //Set Logitech Logo to background color makes screen flash so not used
        public static bool setLogiLogo = false;

        //array for the words displayed in the color picker popup
        public static string[] colorPickerTitles = { "0 Bombs", "1 Bomb", "2 Bombs", "3 Bombs", "4 Bombs", "5 Bombs", "6 Bombs", "Bomb Field", "Covered Field", "Offboard", "Flag", "New Game Key", "Defeat Background", "Victory Background", "Default Background", "Bomb Counter", "Shift Keys" };
    }
}

