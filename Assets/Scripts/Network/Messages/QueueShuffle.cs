using System;

namespace Network.Messages
{
    [Serializable]
    public class QueueShuffle : ANetworkMessage
    {
        public float switchDelay;

        public QueueShuffle(float delay)
        {
            switchDelay = delay;
            lowType = NetworkMessageType.QueueShuffle;
        }
    }
}