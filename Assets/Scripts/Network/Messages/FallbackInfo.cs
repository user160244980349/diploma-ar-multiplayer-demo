using System;

namespace Network.Messages
{
    [Serializable]
    public class FallbackInfo : ANetworkMessage
    {
        public int netKey;
        public float switchDelay;

        public FallbackInfo(int key, float delay)
        {
            netKey = key;
            switchDelay = delay;
            lowType = NetworkMessageType.FallbackInfo;
        }
    }
}