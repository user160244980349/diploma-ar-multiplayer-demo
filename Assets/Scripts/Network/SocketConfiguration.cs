﻿using Network.Delegates;
using UnityEngine.Networking;

namespace Network
{
    public struct SocketConfiguration
    {
        public QosType[] channels;
        public int port;
        public ushort maxConnections;
        public ushort maxMessagesForSend;
        public OnSocketStart onSocketStart;
        public OnConnectEvent onConnectEvent;
        public OnDataEvent onDataEvent;
        public OnBroadcastEvent onBroadcastEvent;
        public OnDisconnectEvent onDisconnectEvent;
        public OnSocketShutdown onSocketDestroy;
    }
}