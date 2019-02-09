namespace Network.Messages
{
    public class Connect : ANetworkMessage
    {
        public Connect()
        {
            lowType = NetworkMessageType.Connect;
        }
    }
}
