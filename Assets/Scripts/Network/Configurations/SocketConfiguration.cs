using UnityEngine.Networking;

namespace Network.Configurations
{
    public struct SocketConfiguration
    {
        public QosType[] channels;
        public int port;
        public ushort maxConnections;
        public int packetSize;
    }
}