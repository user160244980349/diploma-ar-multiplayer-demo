using Network.Messages;
using UnityEngine.Networking;

namespace Network
{
    public struct Error
    {
        public NetworkEventType message;
        public NetworkError type;
    }
}
