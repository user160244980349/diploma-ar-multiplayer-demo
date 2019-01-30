namespace Network.Messages
{
    public class Connect : ANetworkMessage
    {
        public Connect()
        {
            networkMessageType = NetworkMessageType.Connect;
        }
    }
}
