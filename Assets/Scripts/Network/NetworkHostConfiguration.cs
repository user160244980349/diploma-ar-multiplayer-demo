using static Network.NetworkHost;

namespace Network
{
    public struct NetworkHostConfiguration
    {
        public OnNetworkHostStart onNetworkHostStart;
        public OnNetworkHostShutdown onNetworkHostShutdown;
    }
}
