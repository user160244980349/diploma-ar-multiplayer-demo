using System;

namespace Network.Messages
{
    [Serializable]
    public class FallbackHostReady : ANetworkMessage
    {
        public int netKey;

        public FallbackHostReady(int key)
        {
            netKey = key;
            networkMessageType = NetworkMessageType.FallbackHostReady;
        }
    }
}