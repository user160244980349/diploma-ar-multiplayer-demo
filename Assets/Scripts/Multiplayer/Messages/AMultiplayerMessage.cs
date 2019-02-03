using System;
using Network.Messages;

namespace Multiplayer.Messages
{
    [Serializable]
    public abstract class AMultiplayerMessage : ANetworkMessage
    {
        public MultiplayerMessageType highType;

        public AMultiplayerMessage()
        {
            lowType = NetworkMessageType.Higher;
        }
    }
}