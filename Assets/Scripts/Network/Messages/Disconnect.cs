namespace Network.Messages
{
    public class Disconnect : ANetworkMessage
    {
        public Disconnect()
        {
            networkMessageType = NetworkMessageType.Disconnect;
        }
    }
}
