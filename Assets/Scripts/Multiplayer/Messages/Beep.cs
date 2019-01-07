using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public class Boop : AMultiplayerMessage
    {
        public string boop;

        public Boop(string s)
        {
            multiplayerMessageType = MultiplayerMessageType.Beep;
            boop = s;
        }
    }
}
