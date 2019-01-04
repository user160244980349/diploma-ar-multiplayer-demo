namespace Network
{
    public struct Message
    {
        public MessageType type;
        public int length;
        public byte[] data;
    }
}