using System;

namespace Network.Messages
{
    [Serializable]
    public class Beep : ANetworkMessage
    {
        public Beep()
        {
            networkMessageType = NetworkMessageType.Beep;
        }
    }
}