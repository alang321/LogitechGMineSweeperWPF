using System;

namespace LogitechGMineSweeper
{
    public class KeyboardLayout
    {
        static int indexer = 0;

        public SaveFileStatitics SaveFile { get; private set; }
        public string Text { get; private set; }
        //the keyids of the specific keylayouts, Pressing a key in debug mode prints it to the debug console
        public int[] KeyIds { get; private set; }
        public bool[,] EnabledKeys { get; private set; } = new bool[4, 12];
        public object KeyboardDisplayPage { get; set; }
        public int Index { get; private set; }
        public int CoveredFields { get; private set; } = 0;

        public int Easy { get; private set; } = 0;
        public int Medium { get; private set; } = 0;
        public int Hard { get; private set; } = 0;

        public KeyboardLayout(SaveFileStatitics saveFile, string text, int[] keyIds, object keyboardDisplayPage, int easy, int medium, int hard)
        {
            this.KeyIds = keyIds;
            this.SaveFile = saveFile;
            this.Text = text;
            this.KeyboardDisplayPage = keyboardDisplayPage;
            this.Easy = easy;
            this.Medium = medium;
            this.Hard = hard;
            this.Index = indexer++;
            
            for (int i = 0; i < KeyIds.Length - 1; i++)
            {
                if (keyIds[i] == -1) EnabledKeys[ i / 12, i % 12] = false;
                else EnabledKeys[i / 12, i % 12] = true;
            }

            for (int i = 0; i < EnabledKeys.GetLength(0); i++)
            {
                for (int j = 0; j < EnabledKeys.GetLength(1); j++)
                {
                    if (EnabledKeys[i, j]) CoveredFields++;
                }
            }
        }

        public KeyboardLayout(SaveFileStatitics saveFile, string text, int[] keyIds, object keyboardDisplayPage)
        {
            this.KeyIds = keyIds;
            this.SaveFile = saveFile;
            this.Text = text;
            this.KeyboardDisplayPage = keyboardDisplayPage;
            this.Index = indexer++;

            for (int i = 0; i < KeyIds.Length - 1; i++)
            {
                if (keyIds[i] == -1) EnabledKeys[i / 12, i % 12] = false;
                else EnabledKeys[i / 12, i % 12] = true;
            }

            for (int i = 0; i < EnabledKeys.GetLength(0); i++)
            {
                for (int j = 0; j < EnabledKeys.GetLength(1); j++)
                {
                    if (EnabledKeys[i, j]) CoveredFields++;
                }
            }

            this.Easy = Convert.ToInt32(CoveredFields * 0.14);
            this.Medium = Convert.ToInt32(CoveredFields * 0.22);
            this.Hard = Convert.ToInt32(CoveredFields * 0.3);
        }
    }
}
