﻿using System;
using System.IO;

namespace LogitechGMineSweeper
{
    public class SaveFileGlobalStatistics
    {
        public enum SaveIndex { Timer, Total, Win, Defeat }

        public string Path { get; set; }

        string[] GlobalStrings { get; } = { "Wins: ", "Losses: ", "Total: " };

        private int wins;
        private int losses;
        private int total;

        public SaveFileGlobalStatistics(string saveFile)
        {
            this.Path = saveFile;

            if (!File.Exists(Path))
            {
                Directory.CreateDirectory(Directory.GetParent(Path).ToString());
                ResetToDefault();
            }
            else
            {
                try
                {
                    InitValues();
                }
                catch
                {
                    ResetToDefault();
                }
            }
        }
        
        public int Wins
        {
            get
            {
                return wins;
            }
            set
            {
                wins = value;
                string[] globalFile = File.ReadAllLines(Path);
                globalFile[0] = GlobalStrings[0] + wins;
                File.WriteAllLines(Path, globalFile);
            }
        }

        public int Losses
        {
            get
            {
                return losses;
            }
            set
            {
                losses = value;
                string[] globalFile = File.ReadAllLines(Path);
                globalFile[1] = GlobalStrings[1] + losses;
                File.WriteAllLines(Path, globalFile);
            }
        }

        public int Total
        {
            get
            {
                return total;
            }
            set
            {
                total = value;
                string[] globalFile = File.ReadAllLines(Path);
                globalFile[2] = GlobalStrings[2] + total;
                File.WriteAllLines(Path, globalFile);
            }
        }

        private void InitValues()
        {
            string[] globalFile = File.ReadAllLines(Path);

            wins = Convert.ToInt32(globalFile[0].Substring("Wins: ".Length));
            losses = Convert.ToInt32(globalFile[1].Substring("Losses: ".Length));
            total = Convert.ToInt32(globalFile[2].Substring("Total: ".Length));
        }

        public void ResetToDefault()
        {
            string[] file = { GlobalStrings[0] + 0, GlobalStrings[1] + 0, GlobalStrings[2] + 0 };
            File.WriteAllLines(Path, file);
            InitValues();
        }
    }
}
