using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public class LoggedOut : AMultiplayerMessage
    {
        public int PlayerId { get; private set; }

        public LoggedOut(int playerId)
        {
            multiplayerMessageType = MultiplayerMessageType.LoggedOut;
            PlayerId = playerId;
        }
    }
}