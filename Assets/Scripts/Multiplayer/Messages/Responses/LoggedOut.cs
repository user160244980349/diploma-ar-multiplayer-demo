using System;

namespace Multiplayer.Messages.Responses
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