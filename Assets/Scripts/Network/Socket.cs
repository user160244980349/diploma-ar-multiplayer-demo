using UnityEngine.Networking;

namespace Network
{
    public struct Socket
    {
        public bool inUse;
        public HostTopology topology;
        public ConnectionConfig config;
        public Connection[] connections;
        public Delegates.OnConnectEvent onConnectEvent;
        public Delegates.OnDataEvent onDataEvent;
        public Delegates.OnBroadcastEvent onBroadcastEvent;
        public Delegates.OnDisconnectEvent onDisconnectEvent;
    }
}