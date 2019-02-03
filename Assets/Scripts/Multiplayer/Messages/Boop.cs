using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public class Boop : AMultiplayerMessage
    {
        public Boop()
        {
            highType = MultiplayerMessageType.Boop;
        }
    }
}