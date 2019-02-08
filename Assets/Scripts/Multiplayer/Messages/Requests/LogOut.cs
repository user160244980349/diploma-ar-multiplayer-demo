using System;

namespace Multiplayer.Messages.Requests
{
    [Serializable]
    public class LogOut : AMultiplayerMessage
    {
        public int PlayerId { get; private set; }

        public LogOut(int playerId)
        {
            highType = MultiplayerMessageType.LogOut;
            PlayerId = playerId;
        }
    }
}