using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public class LoggedOut : AMultiplayerMessage
    {
        public LoggedOut()
        {
            highType = MultiplayerMessageType.LoggedOut;
        }
    }
}