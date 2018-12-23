using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public struct SocketConfiguration {
        public QosType[] channels;
        public int port;

        public Delegates.Shutdown shutdown;
        public Delegates.Send send;
        public Delegates.OnConnectEvent onConnectEvent;
        public Delegates.OnDataEvent onDataEvent;
        public Delegates.OnBroadcastEvent onBroadcastEvent;
        public Delegates.OnDisconnectEvent onDisconnectEvent;
    }

}

