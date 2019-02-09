using System;

namespace Multiplayer.Messages.Responses
{
    [Serializable]
    public class LoggedIn : AMultiplayerMessage
    {
        public int PlayerId { get; private set; }

        public LoggedIn(int playerId)
        {
            highType = MultiplayerMessageType.LoggedIn;
            PlayerId = playerId;
        }
    }
}