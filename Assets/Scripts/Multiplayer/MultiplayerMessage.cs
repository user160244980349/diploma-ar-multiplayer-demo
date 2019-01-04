namespace Multiplayer
{
    public struct MultiplayerMessage
    {
        public MultiplayerMessageType type;
        public int length;
        public byte[] data;
    }
}