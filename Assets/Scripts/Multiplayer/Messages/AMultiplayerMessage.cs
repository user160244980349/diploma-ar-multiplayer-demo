using Network.Messages;
using System;

namespace Multiplayer.Messages
{
    [Serializable]
    public abstract class AMultiplayerMessage : ANetworkMessage
    {
        public MultiplayerMessageType multiplayerMessageType;

        public AMultiplayerMessage()
        {
            networkMessageType = NetworkMessageType.Higher;
        }
    }
}
