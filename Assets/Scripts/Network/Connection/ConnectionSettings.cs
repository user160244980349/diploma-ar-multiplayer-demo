namespace Network.Connection
{
    public struct ConnectionSettings
    {
        public int id;
        public int socketId;
        public string ip;
        public int port;
        public float connectDelay;
        public float disconnectDelay;
        public float sendRate;
    }
}
