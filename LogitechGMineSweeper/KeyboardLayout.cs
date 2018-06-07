namespace LogitechGMineSweeper
{
    class KeyboardLayout
    {
        static int indexer = 0;

        public SaveFileStatitics SaveFile { get; set; }
        public string Text { get; set; }
        //the keyids of the specific keylayouts, Pressing a key in debug mode prints it to the debug console
        public int[] KeyIds { get; set; }
        public bool[,] EnabledKeys { get; set; }
        public object KeyboardDisplayPage { get; set; }
        public int Index { get; set; }
        public int CoveredFields { get; set; } = 0;

        public int Easy { get; set; } = 0;
        public int Medium { get; set; } = 0;
        public int Hard { get; set; } = 0;

        public KeyboardLayout(SaveFileStatitics saveFile, string text, bool[,] enabledKeys, int[] keyIds, object keyboardDisplayPage, int easy, int medium, int hard)
        {
            this.KeyIds = keyIds;
            this.SaveFile = saveFile;
            this.Text = text;
            this.EnabledKeys = enabledKeys;
            this.KeyboardDisplayPage = keyboardDisplayPage;
            this.Easy = easy;
            this.Medium = medium;
            this.Hard = hard;
            this.Index = indexer++;
            
            for (int i = 0; i < EnabledKeys.GetLength(0); i++)
            {
                for (int j = 0; j < EnabledKeys.GetLength(1); j++)
                {
                    if (EnabledKeys[i, j]) CoveredFields++;
                }
            }
        }

    }
}
