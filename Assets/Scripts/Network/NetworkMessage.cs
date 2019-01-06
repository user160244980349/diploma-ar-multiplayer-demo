using System;

namespace Network
{
    [Serializable]
    public struct NetworkMessage
    {
        public NetworkMessageType type;
        public int length;
        public byte[] data;
    }
}