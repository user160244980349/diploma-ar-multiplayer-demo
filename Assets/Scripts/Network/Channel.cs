using UnityEngine.Networking;

namespace Network {

    public struct Channel {
        public bool inUse;
        public int id;
        public QosType type;
    };

}
