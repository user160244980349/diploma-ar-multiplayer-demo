namespace Network
{
    public enum ConnectionState
    {
        ReadyToConnect,
        WaitingConfirm,
        Connecting,
        Connected,
        Up,
        Disconnecting,
        Disconnected,
    }
}
