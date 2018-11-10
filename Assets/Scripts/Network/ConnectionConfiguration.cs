using UnityEngine;
using System.Collections;

namespace Diploma.Network {

    public class ConnectionConfiguration {

        public string ip = "127.0.0.1";
        public int port = 8000;
        public Socket socket = null;
        public int exceptionConnectionId = 0;
        public int notificationLevel = 1;

    }

}

