namespace Network
{
    public delegate void Shutdown();
    public delegate void Send();
    public delegate void OnConnectEvent(int connection);
    public delegate void OnDataEvent(int connection, byte[] data, int dataSize);
    public delegate void OnBroadcastEvent(int connection);
    public delegate void OnDisconnectEvent(int connection);
}