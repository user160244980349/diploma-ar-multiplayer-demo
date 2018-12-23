using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public struct Connection {
        public bool inUse;
        public int id;
        public ConnectionConfiguration c;
        public bool ready;
    };

}
