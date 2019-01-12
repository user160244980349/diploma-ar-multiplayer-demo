using static Network.Connection;

namespace Network
{
    public struct ConnectionConfiguration
    {
        public int id;
        public int socketId;
        public string ip;
        public int port;
        public OnConnectionStart onConnectionStart;
        public OnConnectionShutdown onConnectionDestroy;
    }
}