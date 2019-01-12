using Network.Delegates;

namespace Network
{
    public struct ClientConfiguration
    {
        public OnClientStart onClientStart;
        public OnClientShutdown onClientShutdown;
    }
}