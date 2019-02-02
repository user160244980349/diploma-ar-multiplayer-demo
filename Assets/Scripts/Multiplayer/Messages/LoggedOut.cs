using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public class LogdedOut : AMultiplayerMessage
    {
        public int PlayerId { get; private set; }

        public LogdedOut(int playerId)
        {
            multiplayerMessageType = MultiplayerMessageType.LoggedOut;
            PlayerId = playerId;
        }
    }
}