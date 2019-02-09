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
        ExitToMainMenu,

        // Network
        ReceiveMultiplayerMessage,
        SendMultiplayerMessage,
        ReceiveNetworkMessage,
        SendNetworkMessage,
        SendNetworkReplyMessage,
        SendNetworkExceptMessage,

        // Multiplayer scene
        RegisterObjectView,
        UnregisterObjectView,

        // Multiplayer triggers
        LoggingIn,
        LoggedIn,
        LoggingOut,
        LoggedOut,
        PlayerRegistered,
        PlayerUnregistered,
        PublishPlayersList,

        // Scene loaded
        SessionStarted,
        SessionEnded,
        StartGame,
    }
}
