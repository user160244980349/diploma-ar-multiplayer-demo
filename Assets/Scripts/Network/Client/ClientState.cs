namespace Network.Client
{
    public enum ClientState
    {
        StartingUp,
        Up,
        WaitingReconnect,
        FallingBack,
        WaitingSwitch,
        ShuttingDown,
        Down,
    }
}
