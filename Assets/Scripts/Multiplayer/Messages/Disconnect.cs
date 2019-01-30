using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public class LogOut : AMultiplayerMessage
    {
        public int PlayerId { get; private set; }

        public LogOut(int playerId)
        {
            multiplayerMessageType = MultiplayerMessageType.Disconnect;
            PlayerId = playerId;
        }
    }
}