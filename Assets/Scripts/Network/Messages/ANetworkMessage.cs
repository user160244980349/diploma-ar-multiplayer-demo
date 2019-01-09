using System;

namespace Network.Messages
{
    [Serializable]
    public abstract class ANetworkMessage
    {
        public NetworkMessageType networkMessageType;
        public int timeStamp;
        public int ping;
    }
}