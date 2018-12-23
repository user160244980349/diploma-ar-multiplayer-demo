using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {
    
    public struct Socket {
        public bool inUse;
        public bool eventsAvaliable;
        public int activeConnections;
        public int activeChannels;
        public SocketConfiguration c;
        public Connection[] connections;
        public Channel[] channels;
    };

}
