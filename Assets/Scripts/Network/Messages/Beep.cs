using System;

namespace Network.Messages
{
    [Serializable]
    public class Beep : ANetworkMessage
    {
        public Beep(string s)
        {
            networkMessageType = NetworkMessageType.Beep;
        }
    }
}