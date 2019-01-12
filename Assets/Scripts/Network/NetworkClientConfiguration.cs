using static Network.NetworkClient;

namespace Network
{
    public struct NetworkClientConfiguration
    {
        public OnNetworkClientStart onNetworkClientStart;
        public OnNetworkClientShutdown onNetworkClientShutdown;
    }
}