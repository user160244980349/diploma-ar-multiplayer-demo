using Network.Messages;

namespace Network.Delegates
{
    #region Connection callbacks
    public delegate void OnConnectionStart(Connection connection);
    public delegate void OnConnectionShutdown(Connection connection);
    #endregion

    #region Socket callbacks
    public delegate void OnSocketStart(Socket socket);
    public delegate void OnConnectEvent(int connection);
    public delegate void OnDataEvent(int connection, ANetworkMessage message);
    public delegate void OnBroadcastEvent(int connection);
    public delegate void OnDisconnectEvent(int connection);
    public delegate void OnSocketShutdown(Socket socket);
    #endregion

    #region Host callbacks
    public delegate void OnHostStart(Host networkHost);
    public delegate void OnHostShutdown(Host networkHost);
    #endregion

    #region Client callbacks
    public delegate void OnClientStart(Client networkClient);
    public delegate void OnClientShutdown(Client networkClient);
    #endregion
}

