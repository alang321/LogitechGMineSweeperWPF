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

        public static string fileConfig = Path.Combine(directory, "config.txt");
        public static string fileUS = Path.Combine(directory, "US.txt");
        public static string fileDE = Path.Combine(directory, "DE.txt");
        public static string fileUK = Path.Combine(directory, "UK.txt");
        public static string fileColors = Path.Combine(directory, "colors.txt");

        public static int bombsDefault = 13;
        public static int keyboardLayout = (int)MineSweeper.Layout.DE;

        public static string[] statisticsDefault = { "", "1: 30:00", "2: 30:00", "3: 30:00", "4: 30:00", "5: 30:00", "6: 30:00", "7: 30:00", "8: 30:00", "9: 30:00", "10: 30:00", "11: 30:00", "12: 30:00", "13: 30:00", "14: 30:00", "15: 30:00", "16: 30:00", "17: 30:00", "18: 30:00", "19: 30:00", "20: 30:00", "21: 30:00", "22: 30:00", "23: 30:00", "24: 30:00", "25: 30:00", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        public static string[] colorsDefault = { "000,000,000", "128,000,128", "255,255,000", "000,128,000", "000,255,255", "000,127,255", "255,000,000", "000,000,255", "255,255,255", "255,200,200", "255,000,255", "255,000,000", "000,000,255", "000,255,255", "255,160,160", "000,255,255", "000,255,255" };
        public static string[] configDefault = { "Wins: 0", "Bombs: " + bombsDefault.ToString(), "Layout: " + keyboardLayout, "Total: 0", "Losses: 0" };

        public static int hard = 13;
        public static int medium = 10;
        public static int easy = 7;

        public static int NumUDminvalue = 5;
        public static int NumUDmaxvalue = 25;
        public static int NumUDstartvalue = bombsDefault;
    }
}
