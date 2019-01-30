namespace Network
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
