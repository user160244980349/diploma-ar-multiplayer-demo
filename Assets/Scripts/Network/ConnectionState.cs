namespace Network
{
    public enum ConnectionState
    {
        StartingUp,
        ReadyToConnect,
        WaitingConfirm,
        WaitingDelay,
        Connecting,
        Connected,
        Up,
        Disconnecting,
        Disconnected,
    }
}
