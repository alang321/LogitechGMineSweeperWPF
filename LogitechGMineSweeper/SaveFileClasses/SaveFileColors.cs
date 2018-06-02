using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogitechGMineSweeper
{
    class SaveFileColors
    {
        public string Path { get; set; }

        public SaveFileColors(string saveFile, ref byte[,] colors)
        {
            this.Path = saveFile;

            if (!File.Exists(Path))
            {
                File.WriteAllLines(Path, Config.colorsDefault);
            }
            else
            {
                colors = SavedColors;
            }
        }

        public byte[,] SavedColors
        {
            get
            {
                string[] colorsFile = File.ReadAllLines(Path);

                byte[,] colors = new byte[17, 3];

                for (int i = 0; i < colorsFile.Length; i++)
                {
                    colors[i, 0] = Convert.ToByte(colorsFile[i].Substring(0, 3));
                    colors[i, 1] = Convert.ToByte(colorsFile[i].Substring(4, 3));
                    colors[i, 2] = Convert.ToByte(colorsFile[i].Substring(8, 3));
                }

                return colors;
            }
            set
            {
                byte[,] colors = value;

                string[] colorsFile = File.ReadAllLines(Path);

                for(int i = 0; i < colorsFile.Length; i++)
                {
                    colorsFile[i] = colors[i, 0].ToString().PadLeft(3, '0') + "," + colors[i, 1].ToString().PadLeft(3, '0') + "," + colors[i, 2].ToString().PadLeft(3, '0');
                }

                File.WriteAllLines(Path, colorsFile);
            }
        }

        public void ResetToDefault(ref byte[,] colors)
        {
            File.WriteAllLines(Path, Config.colorsDefault);
            colors = SavedColors;
        }
    }
}
