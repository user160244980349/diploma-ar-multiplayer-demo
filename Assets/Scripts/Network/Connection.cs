namespace Network
{
    public struct Connection
    {
        public bool inUse;
        public string ip;
        public int port;
        public int exceptionConnectionId;
        public int notificationLevel;
    }
}