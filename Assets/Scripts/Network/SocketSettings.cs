using UnityEngine.Networking;

namespace Network
{
    public class SocketSettings
    {
        public QosType[] channels;
        public int port;
        public ushort maxConnections;
        public int packetSize;
    }
}
