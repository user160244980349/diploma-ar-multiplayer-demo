using Network.Messages;

namespace Network.Delegates
{
    public delegate void OnConnectEvent(int connection);
    public delegate void OnDataEvent(int connection, ANetworkMessage message);
    public delegate void OnBroadcastEvent(int connection);
    public delegate void OnDisconnectEvent(int connection);
}