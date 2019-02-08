using System;

namespace Multiplayer.Messages.Responses
{
    [Serializable]
    public class SessionEnded : AMultiplayerMessage
    {
        public SessionEnded()
        {
            highType = MultiplayerMessageType.SessionEnded;
        }
    }
}