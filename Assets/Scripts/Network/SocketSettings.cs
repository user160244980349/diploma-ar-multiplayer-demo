using UnityEngine.Networking;

namespace Network
{
    public struct SocketSettings
    {
        public QosType[] channels;
        public int port;
        public ushort maxConnections;
        public int packetSize;
    }
}
