using System;

namespace Network.Messages
{
    [Serializable]
    public class FallbackInfo : ANetworkMessage
    {
        public int netKey;
        public int queuePosition;

        public FallbackInfo(int key, int pos)
        {
            netKey = key;
            queuePosition = pos;
            networkMessageType = NetworkMessageType.FallbackInfo;
        }
    }
}