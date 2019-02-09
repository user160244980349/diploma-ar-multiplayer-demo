using System;

namespace Multiplayer.Messages.Responses
{
    [Serializable]
    public class SessionStarted : AMultiplayerMessage
    {
        public SessionStarted()
        {
            highType = MultiplayerMessageType.SessionStarted;
        }
    }
}