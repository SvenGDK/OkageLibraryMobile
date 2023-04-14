using System.Net;

namespace OkageSender
{
    internal class WorkerArgs
    {
        private IPAddress deviceIP;
        private string fileToSend;
        private int chunkSize;
        public byte[] fileSizeBytes;

        public IPAddress DeviceIP { get => deviceIP; set => deviceIP = value; }
        public String FileToSend { get => fileToSend; set => fileToSend = value; }
        public int ChunkSize { get => chunkSize; set => chunkSize = value; }
        public byte[] FileSizeBytes { get => fileSizeBytes; set => fileSizeBytes = value; }
    }
}
