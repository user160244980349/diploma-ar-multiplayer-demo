using System;

namespace Network.Messages
{
    [Serializable]
    public abstract class ANetworkMessage
    {
        public NetworkMessageType type;

        public ANetworkMessage()
        {
            type = NetworkMessageType.None;
        }
    }
}