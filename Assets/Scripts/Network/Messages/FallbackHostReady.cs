using System;

namespace Network.Messages
{
    [Serializable]
    public class FallbackHostReady : ANetworkMessage
    {
        public FallbackHostReady()
        {
            lowType = NetworkMessageType.FallbackHostReady;
        }
    }
}