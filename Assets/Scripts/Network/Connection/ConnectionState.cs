namespace Network.Connection
{
    public enum ConnectionState
    {
        Down,
        ReadyToConnect,
        WaitingDelay,
        WaitingConfirm,
        Connecting,
        Connected,
        Up,
        Disconnecting,
        Disconnected,
    }
}
