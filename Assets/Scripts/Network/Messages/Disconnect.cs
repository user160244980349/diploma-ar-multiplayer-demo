namespace Network.Messages
{
    public class Disconnect : ANetworkMessage
    {
        public Disconnect()
        {
            lowType = NetworkMessageType.Disconnect;
        }
    }
}
