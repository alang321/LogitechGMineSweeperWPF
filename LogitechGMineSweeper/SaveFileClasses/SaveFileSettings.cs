using System;
using System.IO;

namespace LogitechGMineSweeper
{
    public class SaveFileSettings
    {
        public enum SaveIndex { Timer, Total, Win, Defeat }

        public string Path { get; set; }

        string[] SettingStrings { get; } = { "Bombs: ", "Layout: ", "UseBackground: ", "SetLogiLogo: " };

        private int bombs;
        private int layoutIndex;
        private bool useBackground;
        private bool setLogiLogo;

        private int defaultBombs;
        private int defaultLayoutIndex;
        private bool defaultUseBackground;
        private bool defaultSetLogiLogo;

        private int maxBombs;
        private int minBombs;
        private int maxIndex;

        public SaveFileSettings(string saveFile, bool defaultUseBackground, int defaultLayoutIndex, int defaultBombs, bool defaultSetLogiLogo, int minBombs, int maxBombs, int maxIndex)
        {
            this.Path = saveFile;
            this.defaultBombs = defaultBombs;
            this.defaultLayoutIndex = defaultLayoutIndex;
            this.defaultUseBackground = defaultUseBackground;
            this.defaultSetLogiLogo = defaultSetLogiLogo;
            this.maxBombs = maxBombs;
            this.minBombs = minBombs;
            this.maxIndex = maxIndex;

            if (!File.Exists(Path))
            {
                Directory.CreateDirectory(Directory.GetParent(Path).ToString());
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
                settingsFile[0] = SettingStrings[0] + bombs;
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
                settingsFile[1] = SettingStrings[1] + layoutIndex;
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
                settingsFile[2] = SettingStrings[2] + useBackground;
                File.WriteAllLines(Path, settingsFile);
            }
        }

        public bool SetLogiLogo
        {
            get
            {
                return setLogiLogo;
            }
            set
            {
                setLogiLogo = value;
                string[] settingsFile = File.ReadAllLines(Path);
                settingsFile[3] = SettingStrings[3] + setLogiLogo;
                File.WriteAllLines(Path, settingsFile);
            }
        }

        private void InitValues()
        {
            string[] settingsFile = File.ReadAllLines(Path);

            string b = settingsFile[2].Substring(SettingStrings[2].Length);
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
                throw new Exception("Invalid useBackground Value");
            }

            b = settingsFile[3].Substring(SettingStrings[3].Length);
            if (b == "False")
            {
                setLogiLogo = false;
            }
            else if (b == "True")
            {
                setLogiLogo = true;
            }
            else
            {
                throw new Exception("Invalid useBackground Value");
            }

            bombs = Convert.ToInt32(settingsFile[0].Substring(SettingStrings[0].Length));
            if(bombs < minBombs || bombs > maxBombs)
            {
                throw new Exception("Invalid bombs Value");
            }

            layoutIndex = Convert.ToInt32(settingsFile[1].Substring(SettingStrings[1].Length));
            if (layoutIndex < 0 || layoutIndex > maxIndex)
            {
                throw new Exception("Invalid layoutIndex Value");
            }
        }

        public void ResetToDefault()
        {
            string[] file = { SettingStrings[0] + defaultBombs, SettingStrings[1] + defaultLayoutIndex, SettingStrings[2] + defaultUseBackground, SettingStrings[3] + defaultSetLogiLogo };
            File.WriteAllLines(Path, file);
            InitValues();
        }
    }
}
