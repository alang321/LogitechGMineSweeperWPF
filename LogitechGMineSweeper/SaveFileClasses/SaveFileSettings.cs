using System;
using System.IO;

namespace LogitechGMineSweeper
{
    class SaveFileSettings
    {
        public enum SaveIndex { Timer, Total, Win, Defeat }

        public string Path { get; set; }
        
        private int bombs;
        private int layoutIndex;
        private bool useBackground;

        public SaveFileSettings(string saveFile)
        {
            this.Path = saveFile;

            if (!File.Exists(Path))
            {
                Directory.CreateDirectory(Config.Directory);
                ResetToDefault();
            }
            else
            {
                string[] configFile = File.ReadAllLines(Path);

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

        public int Bombs
        {
            get
            {
                return bombs;
            }
            set
            {
                bombs = value;
                string[] settingsFile = File.ReadAllLines(Path);
                settingsFile[0] = "Bombs: " + bombs;
                File.WriteAllLines(Path, settingsFile);
            }
        }


        public int LayoutIndex
        {
            get
            {
                return layoutIndex;
            }
            set
            {
                layoutIndex = value;
                string[] settingsFile = File.ReadAllLines(Path);
                settingsFile[1] = "Layout: " + layoutIndex;
                File.WriteAllLines(Path, settingsFile);
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
                string[] settingsFile = File.ReadAllLines(Path);
                settingsFile[2] = "UseBackground: " + useBackground;
                File.WriteAllLines(Path, settingsFile);
            }
        }

        private void InitValues()
        {
            string[] settingsFile = File.ReadAllLines(Path);

            string b = settingsFile[2].Substring("UseBackground: ".Length);
            if (b == "False")
            {
                useBackground = false;
            }
            else if (b == "True")
            {
                useBackground = true;
            }
            else
            {
                ResetToDefault();
                return;
            }
            
            bombs = Convert.ToInt32(settingsFile[0].Substring("Bombs: ".Length));
            if(bombs < Config.MinBombs || bombs > Config.MaxBombs)
            {
                ResetToDefault();
                return;
            }

            layoutIndex = Convert.ToInt32(settingsFile[1].Substring("Layout: ".Length));
            if (layoutIndex < 0 || layoutIndex > Config.KeyboardLayouts.Length-1)
            {
                ResetToDefault();
                return;
            }
        }

        public void ResetToDefault()
        {
            File.WriteAllLines(Path, Config.SettingsDefault);
            InitValues();
        }
    }
}
