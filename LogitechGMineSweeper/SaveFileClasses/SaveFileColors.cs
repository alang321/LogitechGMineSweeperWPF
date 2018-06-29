using System;
using System.IO;

namespace LogitechGMineSweeper
{
    public class SaveFileColors
    {
        public string Path { get; set; }

        byte[,] savedColors;
        byte[,] defaultColors;

        public SaveFileColors(string saveFile, byte[,] defaultColors)
        {
            this.Path = saveFile;
            this.defaultColors = defaultColors;

            if (!File.Exists(Path))
            {
                Directory.CreateDirectory(Directory.GetParent(Path).ToString());
                ResetToDefault();
            }
            else
            {
                try
                {
                    InitSavedColors();
                }
                catch
                {
                    ResetToDefault();
                }
            }
        }
        
        private void InitSavedColors()
        {
            string[] colorsFile = File.ReadAllLines(Path);

            savedColors = new byte[17, 3];

            for (int i = 0; i < colorsFile.Length; i++)
            {
                savedColors[i, 0] = Convert.ToByte(colorsFile[i].Substring(0, 3));
                savedColors[i, 1] = Convert.ToByte(colorsFile[i].Substring(4, 3));
                savedColors[i, 2] = Convert.ToByte(colorsFile[i].Substring(8, 3));
            }
        }

        public byte[,] SavedColors
        {
            get
            {
                return savedColors;
            }
            set
            {
                byte[,] colors = value;

                string[] colorsFile = new string[17];

                for(int i = 0; i < colorsFile.Length; i++)
                {
                    colorsFile[i] = colors[i, 0].ToString().PadLeft(3, '0') + "," + colors[i, 1].ToString().PadLeft(3, '0') + "," + colors[i, 2].ToString().PadLeft(3, '0');
                }

                File.WriteAllLines(Path, colorsFile);
            }
        }

        public void ResetToDefault()
        {
            SavedColors = defaultColors;
            InitSavedColors();
        }
    }
}
