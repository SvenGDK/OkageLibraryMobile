namespace OkageSender
{
    internal class ELFFile
    {
        private string fileName;
        private string console;
        private string firmware;
        private string filePath;

        public string FileName { get => fileName; set => fileName = value; }
        public string Console { get => console; set => console = value; }
        public string Firmware { get => firmware; set => firmware = value; }
        public string FilePath { get => filePath; set => filePath = value; }

    }
}
