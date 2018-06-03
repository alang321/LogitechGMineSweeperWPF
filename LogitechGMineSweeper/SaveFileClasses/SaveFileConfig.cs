using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogitechGMineSweeper
{
    class SaveFileConfig
    {
        public enum SaveIndex { Timer, Total, Win, Defeat }

        public string Path { get; set; }

        private int wins;
        private int bombs;
        private int layout;
        private int total;
        private int losses;
        private bool useBackground;

        public SaveFileConfig(string saveFile)
        {
            this.Path = saveFile;

            if (!File.Exists(Path))
            {
                Directory.CreateDirectory(Config.directory);
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
                string[] configFile = File.ReadAllLines(Path);
                configFile[0] = "Wins: " + wins;
                File.WriteAllLines(Path, configFile);
            }
        }

        public int Bombs
        {
            get
            {
                return bombs;
            }
            set
            {
                bombs = value;
                string[] configFile = File.ReadAllLines(Path);
                configFile[1] = "Bombs: " + bombs;
                File.WriteAllLines(Path, configFile);
            }
        }

        public int Layout
        {
            get
            {
                return layout;
            }
            set
            {
                layout = value;
                string[] configFile = File.ReadAllLines(Path);
                configFile[2] = "Layout: " + layout;
                File.WriteAllLines(Path, configFile);
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
                string[] configFile = File.ReadAllLines(Path);
                configFile[3] = "Total: " + total;
                File.WriteAllLines(Path, configFile);
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
                string[] configFile = File.ReadAllLines(Path);
                configFile[4] = "Losses: " + losses;
                File.WriteAllLines(Path, configFile);
            }
        }

        public bool UseBackground
        {
            get
            {
                return useBackground;
            }
            set
            {
                useBackground = value;
                string[] configFile = File.ReadAllLines(Path);
                configFile[5] = "UseBackground: " + useBackground;
                File.WriteAllLines(Path, configFile);
            }
        }

        private void InitValues()
        {
            string[] configFile = File.ReadAllLines(Path);
            string b = configFile[5].Substring("UseBackground: ".Length);
            if (b == "False")
            {
                useBackground = false;
            }
            else
            {
                useBackground = true;
            }

            wins = Convert.ToInt32(configFile[0].Substring("Wins: ".Length));
            bombs = Convert.ToInt32(configFile[1].Substring("Bombs: ".Length));
            layout = Convert.ToInt32(configFile[2].Substring("Layout: ".Length));
            total = Convert.ToInt32(configFile[3].Substring("Total: ".Length));
            losses = Convert.ToInt32(configFile[4].Substring("Losses: ".Length));
        }

        public void GetAllValues(ref int bombs, ref int keyLayout, ref bool useBackground)
        {
            bombs = Bombs;
            keyLayout = Layout;
            useBackground = UseBackground;
        }

        public void UpdateSaveFile(int bombs, int keyLayout, bool useBackground)
        {
            string[] lines = { "Wins: " + wins, "Bombs: " + bombs, "Layout: " + keyLayout, "Total: " + total, "Losses: " + losses, "UseBackground: " + useBackground };
            File.WriteAllLines(Path, lines);
        }

        public void ResetToDefault()
        {
            File.WriteAllLines(Path, Config.configDefault);
            InitValues();
        }
    }
}
