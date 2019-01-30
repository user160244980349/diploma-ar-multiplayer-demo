using UnityEngine.Networking;

namespace Network.Messages
{
    public struct MessageWrapper
    {
        public ANetworkMessage message;
        public string ip;
        public int port;
        public int connection;
        public int ping;
    }
}
