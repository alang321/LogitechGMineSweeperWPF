using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogitechGMineSweeper
{
    static class SaveFile
    {
        public delegate void PrintStatsEventHandler();

        public static event PrintStatsEventHandler PrintStatsEvent;

        public enum SaveIndex { Timer, Total, Win, Defeat }

        public static int ReadSaveFile(string saveFile, int bombs, int index)
        {
            string[] file = File.ReadAllLines(saveFile);

            int index1 = file[bombs].IndexOf("." + index.ToString());
            int index2 = file[bombs].IndexOf("." + (index + 1).ToString());

            return Convert.ToInt32(file[bombs].Substring(index1 + 2, (index2 - index1 - 2)));
        }

        public static void UpdateSaveFile(string saveFile, int bombs, int index, int newValue)
        {
            string[] file = File.ReadAllLines(saveFile);

            int index1 = file[bombs].IndexOf("." + index.ToString());
            int index2 = file[bombs].IndexOf("." + (index + 1).ToString());

            file[bombs] = file[bombs].Substring(0, index1 + 2) + newValue + file[bombs].Substring(index2);

            File.WriteAllLines(saveFile, file);
        }

        public static void IncrementSaveFile(string saveFile, int bombs, int index)
        {
            string[] file = File.ReadAllLines(saveFile);

            int index1 = file[bombs].IndexOf("." + index.ToString());
            int index2 = file[bombs].IndexOf("." + (index + 1).ToString());

            file[bombs] = file[bombs].Substring(0, index1 + 2) + (Convert.ToInt32(file[bombs].Substring(index1 + 2, (index2 - index1 - 2))) + 1) + file[bombs].Substring(index2);

            File.WriteAllLines(saveFile, file);

            PrintStatsEvent();
        }

        public static void MigrateOldSave(string saveFile, int bombs, int index)
        {
            //todo
        }
    }
}
