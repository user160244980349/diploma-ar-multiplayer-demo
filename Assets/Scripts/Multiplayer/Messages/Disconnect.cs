using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public class Disconnect : AMultiplayerMessage
    {
        public int PlayerId { get; private set; }

        public Disconnect(int playerId)
        {
            multiplayerMessageType = MultiplayerMessageType.Disconnect;
            PlayerId = playerId;
        }
    }
}