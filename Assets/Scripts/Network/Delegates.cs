using Network.Configurations;
using Network.Messages;

namespace Network.Delegates
{
    #region Connection callbacks
    public delegate void OnConnectionStart(int connectionId);
    public delegate void OnConnectionWaitingConfirm(Connection connection);
    public delegate void OnConnectionShutdown(int connectionId);
    #endregion

    #region Socket callbacks
    public delegate void OnSocketStart(Socket socket);
    public delegate void OnConnectEvent(int connectionId);
    public delegate void OnDataEvent(int connectionId, ANetworkMessage message);
    public delegate void OnBroadcastEvent(ConnectionConfiguration cc, ANetworkMessage message);
    public delegate void OnDisconnectEvent(int connectionId);
    public delegate void OnSocketShutdown(int socketId);
    #endregion

    #region Host callbacks
    public delegate void OnHostStart(Host networkHost);
    public delegate void OnHostShutdown();
    #endregion

    #region Client callbacks
    public delegate void OnClientStart(Client networkClient);
    public delegate void OnClientFallback(int broadcastKey);
    public delegate void OnClientShutdown();
    #endregion
}

