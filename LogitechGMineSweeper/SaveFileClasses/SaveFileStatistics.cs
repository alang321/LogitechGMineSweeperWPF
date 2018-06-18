using System;
using System.IO;

namespace LogitechGMineSweeper
{
    public class SaveFileStatitics
    {
        public enum SaveIndex { Timer, Total, Win, Defeat }

        public string Path { get; set; }

        public SaveFileStatitics(string saveFile)
        {
            this.Path = saveFile;

            if (!File.Exists(Path))
            {
                Directory.CreateDirectory(Config.Directory);
                File.WriteAllLines(Path, Config.StatisticsDefault);
            }
            else
            {
                string[] lines = File.ReadAllLines(Path);

                if (lines[0].Length < 5)
                {
                    try
                    {
                        MigrateOldSave();
                    }
                    catch
                    {
                        File.WriteAllLines(Path, Config.StatisticsDefault);
                    }
                }
                else if (lines.Length == 49)
                {
                    ChangeOldDefaultTime();
                }
            }
        }

        #region Read

        public int GetBestTime(int bombs)
        {
            return ReadSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Timer);
        }

        public int GetWins(int bombs)
        {
            return ReadSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Win);
        }

        public int GetLosses(int bombs)
        {
            return ReadSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Defeat);
        }

        public int GetTotal(int bombs)
        {
            return ReadSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Total);
        }

        private int ReadSaveFile(int bombs, int index)
        {
            string[] file = File.ReadAllLines(Path);

            int index1 = file[bombs].IndexOf("." + index.ToString());
            int index2 = file[bombs].IndexOf("." + (index + 1).ToString());

            return Convert.ToInt32(file[bombs].Substring(index1 + 2, (index2 - index1 - 2)));
        }

        #endregion

        #region Update

        public void UpdateBestTime(int bombs, int newValue)
        {
            UpdateSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Timer, newValue);
        }

        public void UpdateWins(int bombs, int newValue)
        {
            UpdateSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Win, newValue);
        }

        public void UpdateLosses(int bombs, int newValue)
        {
            UpdateSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Defeat, newValue);
        }

        public void UpdateTotal(int bombs, int newValue)
        {
            UpdateSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Total, newValue);
        }

        private void UpdateSaveFile(int bombs, int index, int newValue)
        {
            string[] file = File.ReadAllLines(Path);

            int index1 = file[bombs].IndexOf("." + index.ToString());
            int index2 = file[bombs].IndexOf("." + (index + 1).ToString());

            file[bombs] = file[bombs].Substring(0, index1 + 2) + newValue + file[bombs].Substring(index2);

            File.WriteAllLines(Path, file);
        }

        #endregion

        #region Increment

        public void IncrementWins(int bombs)
        {
            IncrementSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Win);
        }

        public void IncrementLosses(int bombs)
        {
            IncrementSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Defeat);
        }

        public void IncrementTotal(int bombs)
        {
            IncrementSaveFile(bombs, (int)SaveFileStatitics.SaveIndex.Total);
        }

        private void IncrementSaveFile(int bombs, int index)
        {
            string[] file = File.ReadAllLines(Path);

            int index1 = file[bombs].IndexOf("." + index.ToString());
            int index2 = file[bombs].IndexOf("." + (index + 1).ToString());

            file[bombs] = file[bombs].Substring(0, index1 + 2) + (Convert.ToInt32(file[bombs].Substring(index1 + 2, (index2 - index1 - 2))) + 1) + file[bombs].Substring(index2);

            File.WriteAllLines(Path, file);
        }

        #endregion

        public void ResetToDefault()
        {
            File.WriteAllLines(Path, Config.StatisticsDefault);
        }

        public void MigrateOldSave()
        {
            int minutes = 0;
            int seconds = 0;

            //+42 total game +21 defeat game +0 victory
            string[] lines = File.ReadAllLines(Path);
            string[] newFile = new string[Config.StatisticsDefault.Length];

            for (int i = 0; i < Config.StatisticsDefault.Length; i++)
            {
                newFile[i] = Config.StatisticsDefault[i];
            }

            for (int i = 5; i <= 25; i++)
            {
                minutes = Convert.ToInt32(lines[i].Substring(2 + i.ToString().Length, 2));
                seconds = Convert.ToInt32(lines[i].Substring(5 + i.ToString().Length, 2));
                newFile[i] = i + ": .0" + ((minutes * 60000) + (seconds * 1000)) + ".1" + lines[i + 63] + ".2" + lines[i + 21] + ".3" + lines[i + 42] + ".4";
            }

            foreach(string line in newFile)
            {
                line.Replace(".01800000", ".0-1");
            }
            
            File.WriteAllLines(Path, newFile);
        }


        public void ChangeOldDefaultTime()
        {
            string[] lines = File.ReadAllLines(Path);

            for(int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace(".01800000", ".0-1");
            }

            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = Config.Version;

            File.WriteAllLines(Path, lines);
        }
    }
}
