using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class SocketConfiguration {

        public QosType[] channels = { QosType.Reliable };
        public int maxConnections = 16;
        public int port = 8000;
        public int bufferSize = 1024;

    }

}

