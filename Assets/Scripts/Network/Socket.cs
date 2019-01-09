using Network.Delegates;
using UnityEngine.Networking;

namespace Network
{
    public struct Socket
    {
        public bool inUse;
        public HostTopology topology;
        public ConnectionConfig config;
        public Connection[] connections;
        public OnConnectEvent onConnectEvent;
        public OnDataEvent onDataEvent;
        public OnBroadcastEvent onBroadcastEvent;
        public OnDisconnectEvent onDisconnectEvent;
    }
}