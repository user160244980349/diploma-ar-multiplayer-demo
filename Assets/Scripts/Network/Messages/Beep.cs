using System;

namespace Network.Messages
{
    [Serializable]
    public class Beep : ANetworkMessage
    {
        public string boop;

        public Beep(string s)
        {
            networkMessageType = NetworkMessageType.Beep;
            boop = s;
        }
    }
}
