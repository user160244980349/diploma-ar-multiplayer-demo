using System;

namespace Multiplayer
{
    [Serializable]
    public struct MultiplayerMessage
    {
        public MultiplayerMessageType type;
        public int length;
        public byte[] data;
    }
}