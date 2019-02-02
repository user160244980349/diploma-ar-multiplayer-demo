using System;
using UnityEngine;

namespace Multiplayer.Messages
{
    [Serializable]
    public class LoggedIn : AMultiplayerMessage
    {
        public int PlayerId{ get; private set; }

        public LoggedIn(int playerId)
        {
            multiplayerMessageType = MultiplayerMessageType.LoggedIn;
            PlayerId = playerId;
        }
    }
}