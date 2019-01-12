using Network.Delegates;

namespace Network
{
    public struct HostConfiguration
    {
        public OnHostStart onHostStart;
        public OnHostShutdown onHostShutdown;
    }
}
