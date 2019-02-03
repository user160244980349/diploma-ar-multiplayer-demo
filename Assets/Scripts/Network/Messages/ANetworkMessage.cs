using System;

namespace Network.Messages
{
    [Serializable]
    public abstract class ANetworkMessage
    {
        public NetworkMessageType lowType;
        public int timeStamp;
    }
}