namespace Network.Messages.Wrappers
{
    public struct ReceiveWrapper
    {
        public ANetworkMessage message;
        public string ip;
        public int port;
        public int connection;
        public int ping;
    }
}
