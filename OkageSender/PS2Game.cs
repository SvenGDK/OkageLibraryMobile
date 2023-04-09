namespace OkageSender
{
    internal class PS2Game
    {
        private string title;
        private string gameID;
        private string region;
        private string filePath;

        public string Title { get => title; set => title = value; }
        public string GameID { get => gameID; set => gameID = value; }
        public string Region { get => region; set => region = value; }
        public string FilePath { get => filePath; set => filePath = value; }

    }
}
