using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogitechGMineSweeper
{
    class KeyboardLayout
    {
        public SaveFileStatitics SaveFile { get; set; }
        public string Text { get; set; }
        //the keyids of the specific keylayouts, Pressing a key in debug mode prints it to the debug console
        public int[] KeyIds { get; set; }
        public bool[,] EnabledKeys { get; set; }
        public object KeyboardDisplayPage { get; set; }
        public int Index { get; set; }

        public KeyboardLayout(SaveFileStatitics saveFile, string text, bool[,] enabledKeys, int[] keyIds, object keyboardDisplayPage, int index)
        {
            this.KeyIds = keyIds;
            this.SaveFile = saveFile;
            this.Text = text;
            this.EnabledKeys = enabledKeys;
            this.KeyboardDisplayPage = keyboardDisplayPage;
            this.Index = index;
        }

    }
}
