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

        //to add layout add everything in here, pretty self explaining
        //the keyLayoutEnabledKeys array controls which keys are enabled, here for example on the us keyboard the bootom left is disabled as it is not present on the keyboard
        //then add in app.xaml.cs a custom assign paramteter function
        //in minwindow.xaml add custom keyboard display like its been with the other ones, check the rightmost key in the playing field for example
        //in mainwindow.xaml.cs in Button_Click_1 do it like its been done just check the code
        //add in mainwindow.xaml in the settings tab item the option for your layout
        public enum Layout{ DE, US, UK }
        
        public static string[] fileStatistics = { Path.Combine(directory, "DE.txt"), Path.Combine(directory, "US.txt"), Path.Combine(directory, "UK.txt") };
        public static string[] textForLayout = { "DE", "US", "UK" };

        public static int bombsDefault = 14;
        public static int keyboardLayoutDefault = (int)Config.Layout.DE;
        public static bool useBackgroundDefault = false;
        
        public static bool[,,] keyLayoutEnabledKeys =
        //DE
        {{{ true, true, true, true, true, true, true, true, true, true, true, true },
           { true, true, true, true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true, true, true, true },
           { true, true, true, true, true, true, true, true, true, true, true,false }},

        //US
        {{ true, true, true, true, true, true, true, true, true, true, true, true },
           { true, true, true, true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true, true, true,false },
           {false, true, true, true, true, true, true, true, true, true, true,false }},

        //UK
        {{ true, true, true, true, true, true, true, true, true, true, true, true },
           { true, true, true, true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true, true, true, true },
           { true, true, true, true, true, true, true, true, true, true, true,false }}};

        public static string[] statisticsDefault = { "", "1: 30:00", "2: 30:00", "3: 30:00", "4: 30:00", "5: 30:00", "6: 30:00", "7: 30:00", "8: 30:00", "9: 30:00", "10: 30:00", "11: 30:00", "12: 30:00", "13: 30:00", "14: 30:00", "15: 30:00", "16: 30:00", "17: 30:00", "18: 30:00", "19: 30:00", "20: 30:00", "21: 30:00", "22: 30:00", "23: 30:00", "24: 30:00", "25: 30:00", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        public static string[] colorsDefault = { "000,000,000", "255,000,000", "255,255,000", "000,128,000", "000,255,255", "000,127,255", "128,000,128", "000,000,255", "255,255,255", "255,200,200", "255,000,255", "255,000,000", "000,000,255", "000,255,255", "255,160,160", "000,255,255", "255,000,255" };
        public static string[] configDefault = { "Wins: 0", "Bombs: " + bombsDefault.ToString(), "Layout: " + keyboardLayoutDefault, "Total: 0", "Losses: 0", "UseBackground: " + useBackgroundDefault };

        public static int hard = 14;
        public static int medium = 10;
        public static int easy = 7;

        public static int maxBombs = 25;
        public static int minBombs = 5;

        public static int NumUDstartvalue = bombsDefault;
    }
}

