using System;

namespace Multiplayer.Messages.Requests
{
    [Serializable]
    public class LogIn : AMultiplayerMessage
    {
        public string PlayerName { get; private set; }

        public LogIn(string name)
        {
            highType = MultiplayerMessageType.LogIn;
            PlayerName = name;
        }
    }
}