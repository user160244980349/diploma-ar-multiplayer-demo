namespace Events
{
    public enum GameEventType
    {
        // Host triggers
        HostStarted,
        HostStartedInFallback,
        WaitPlayers,
        HostDestroyed,

        // Client triggers
        ClientStarted,
        FoundLobby,
        ConnectedToHost,
        ConnectedToHostInFallback,
        DisconnectedFromHost,
        DisconnectedFromHostInFallback,
        ClientDestroyed,

        // NetworkManager Commands
        RegisterSocket,
        UnregisterSocket,
        StartHost,
        StartClient,
        DestroyHost,
        DestroyClient,
        Switch,

        // ApplicationManager Commands
        ResetMultiplayerManager,

        // Host Commands
        StartLobbyBroadcast,
        StopLobbyBroadcast,

        // Client Commands
        ConnectToHost,
        DisconnectFromHost,

        // Buttons triggers
        BecomeHost,
        CreateLobby,
        BecomeClient,
        ConnectToLobby,
        LogIntoLobby,
        LogOutLobby,

        // Network
        ReceiveMultiplayerMessage,
        SendMultiplayerMessage,
        ReceiveNetworkMessage,
        SendNetworkMessage,

        // Multiplayer triggers
        LoggingIn,
        LoggedIn,
        LoggingOut,
        LoggedOut,
    }
}
