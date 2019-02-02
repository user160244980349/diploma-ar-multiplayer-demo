namespace Network
{
    public enum ConnectionState
    {
        StartingUp,
        ReadyToConnect,
        Connecting,
        WaitingConfirm,
        Connected,
        Up,
        Disconnecting,
        WrongConnection,
        Disconnected,
    }
}
