using Network.Messages;

namespace Network.Delegates
{
    #region Connection callbacks
    public delegate void OnConnectionStart(Connection connection);
    public delegate void OnConnectionShutdown(int connectionId);
    #endregion

    #region Socket callbacks
    public delegate void OnSocketStart(Socket socket);
    public delegate void OnConnectEvent(int connectionId);
    public delegate void OnDataEvent(int connectionId, ANetworkMessage message);
    public delegate void OnBroadcastEvent(int connectionId);
    public delegate void OnDisconnectEvent(int connectionId);
    public delegate void OnSocketShutdown(int socketId);
    #endregion

    #region Host callbacks
    public delegate void OnHostStart(Host networkHost);
    public delegate void OnHostShutdown();
    #endregion

    #region Client callbacks
    public delegate void OnClientStart(Client networkClient);
    public delegate void OnClientShutdown();
    #endregion
}

