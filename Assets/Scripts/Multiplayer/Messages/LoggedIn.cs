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
            highType = MultiplayerMessageType.LoggedIn;
            PlayerId = playerId;
        }
    }
}