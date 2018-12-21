using UnityEngine.Networking;

namespace Diploma.Network {

    public struct Channel
    {
        public bool inUse;
        public int id;
        public QosType type;
    };

}
