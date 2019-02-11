using Events;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager Singleton { get; private set; }

    private bool _hosting;
    private GameObject _multiplayerManager;

    private void Awake()
    {
        name = "ApplicationManager";
        if (Singleton == null)
            Singleton = this;
        else if (Singleton == this) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        var _eventManager = Resources.Load("Managers/EventManager") as GameObject;
        var _networkManager = Resources.Load("Managers/NetworkManager") as GameObject;
        var _consoleManager = Resources.Load("Managers/ConsoleManager") as GameObject;
        _multiplayerManager = Resources.Load("Managers/MultiplayerManager") as GameObject;

        Instantiate(_eventManager);
        Instantiate(_networkManager);
        Instantiate(_consoleManager);
        Instantiate(_multiplayerManager);

        Application.targetFrameRate = 240;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        // Button triggers
        EventManager.Singleton.Subscribe(GameEventType.BecomeHost, OnBecomeHost);
        EventManager.Singleton.Subscribe(GameEventType.CreateLobby, OnCreateLobby);
        EventManager.Singleton.Subscribe(GameEventType.BecomeClient, OnBecomeClient);
        EventManager.Singleton.Subscribe(GameEventType.ConnectToLobby, OnConnectToLobby);
        EventManager.Singleton.Subscribe(GameEventType.LogIntoLobby, OnLogIntoLobby);
        EventManager.Singleton.Subscribe(GameEventType.LoggedIn, OnLoggedIn);
        EventManager.Singleton.Subscribe(GameEventType.SessionStarted, OnSessionStarted);
        EventManager.Singleton.Subscribe(GameEventType.LogOutLobby, OnLogOutLobby);
        EventManager.Singleton.Subscribe(GameEventType.LoggedOut, OnLoggedOut);
        EventManager.Singleton.Subscribe(GameEventType.ExitToMainMenu, OnExitToMainMenu);

        // Host events
        EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
        EventManager.Singleton.Subscribe(GameEventType.HostStartedInFallback, OnHostStartedInFallback);
        EventManager.Singleton.Subscribe(GameEventType.HostDestroyed, OnHostDestroyed);

        // Client events
        EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
        EventManager.Singleton.Subscribe(GameEventType.ConnectedToHost, OnConnectedToHost);
        EventManager.Singleton.Subscribe(GameEventType.DisconnectedFromHost, OnDisconnectedFromHost);

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Button triggers
    private void OnBecomeHost(object info)
    {
        EventManager.Singleton.Publish(GameEventType.StartHost, null);
    }
    private void OnCreateLobby(object info)
    {
        EventManager.Singleton.Publish(GameEventType.StartLobbyBroadcast, info);
        SceneManager.LoadScene("LogIn", LoadSceneMode.Single);
    }
    private void OnBecomeClient(object info)
    {
        EventManager.Singleton.Publish(GameEventType.StartClient, null);
    }
    private void OnConnectToLobby(object info)
    {
        EventManager.Singleton.Publish(GameEventType.ConnectToHost, info);
    }
    private void OnLogIntoLobby(object info)
    {
        EventManager.Singleton.Publish(GameEventType.LoggingIn, info);
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }
    private void OnLoggedIn(object info)
    {
        if (_hosting)
            SceneManager.LoadScene("WaitPlayers", LoadSceneMode.Single);
        else
            SceneManager.LoadScene("WaitStart", LoadSceneMode.Single);
    }
    private void OnSessionStarted(object info)
    {
        if (_hosting)
            EventManager.Singleton.Publish(GameEventType.StopLobbyBroadcast, null);

        if (Application.platform == RuntimePlatform.Android)
            SceneManager.LoadScene("PlaygroundAR", LoadSceneMode.Single);

        if (Application.platform == RuntimePlatform.WindowsPlayer)
            SceneManager.LoadScene("Playground", LoadSceneMode.Single);
    }
    private void OnLogOutLobby(object info)
    {
        EventManager.Singleton.Publish(GameEventType.LoggingOut, null);
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }
    private void OnLoggedOut(object info)
    {
        if (_hosting)
            EventManager.Singleton.Publish(GameEventType.DestroyHost, null);
        else
            EventManager.Singleton.Publish(GameEventType.DestroyClient, null);
    }
    private void OnExitToMainMenu(object info)
    {
        Instantiate(_multiplayerManager);
        if (_hosting)
        {
            EventManager.Singleton.Publish(GameEventType.StopLobbyBroadcast, null);
            EventManager.Singleton.Publish(GameEventType.DestroyHost, null);
        }
        else
        {
            EventManager.Singleton.Publish(GameEventType.DestroyClient, null);
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }

    // Host events
    private void OnHostStarted(object info)
    {
        _hosting = true;
        SceneManager.LoadScene("LobbyCreation", LoadSceneMode.Single);
    }
    private void OnHostStartedInFallback(object info)
    {
        _hosting = true;
    }
    private void OnHostDestroyed(object info)
    {
        Instantiate(_multiplayerManager);
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    // Client events
    private void OnClientStarted(object info)
    {
        _hosting = false;
        SceneManager.LoadScene("LobbyDiscovery", LoadSceneMode.Single);
    }
    private void OnConnectedToHost(object info)
    {
        SceneManager.LoadScene("LogIn", LoadSceneMode.Single);
    }
    private void OnDisconnectedFromHost(object info)
    {
        Instantiate(_multiplayerManager);
        EventManager.Singleton.Publish(GameEventType.DestroyClient, null);
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (loadSceneMode != LoadSceneMode.Single) return;
        switch (scene.name)
        {
            case "Playground":
            {
                EventManager.Singleton.Publish(GameEventType.StartGame, null);
                break;
            }
            case "PlaygroundAR":
            {
                EventManager.Singleton.Publish(GameEventType.StartGame, null);
                break;
            }
            case "WaitPlayers":
            {
                EventManager.Singleton.Publish(GameEventType.PublishPlayersList, null);
                break;
            }
            case "WaitStart":
            {
                EventManager.Singleton.Publish(GameEventType.PublishPlayersList, null);
                break;
            }
        }
    }
}

/**
 *
 * TODO:
 * 
 * + Refactor
 * 
 * + Event based interfaces
 * 
 * - Bugs:
 *     + Reset multiplayer manager
 *     + Fps unlock
 *     + Phantom connection
 *     + Disconnected not unregistring
 *     + Finish players list in a lobby
 *     + Delete singleton from multiplayer scene
 *     - Use logout on timed out players
 *     - Exit if not logged in
 *     - (OPTIONAL) Player name above model
 *     - (OPTIONAL) Fix structs boxing
 *     + (OPTIONAL) Replace some of classes with structs
 *     + (OPTIONAL) Add prefered channel to message types
 *     - (OPTIONAL) Maybe event manager rework
 *     - (OPTIONAL) Errors queue
 * 
 * - Multiplayer:
 * 
 *     + Messages:
 *         + Connection
 *         + Disconnection
 *         + Serialize/Deserialize in formatter
 *         + Smth with message structure (PlayerID add)
 *     + Player identification via ID
 *     + Player spawn
 *     + MultiplayerManager Serialization/Deserialization
 *     + Synchronization logic
 *     + Divide host and client logic by parts
 *     + (OPTIONAL) Smoother interpolation ways 
 *
 * - Playground:
 *
 *     - Color picker for login dialog
 *     + Non-controllable physical objects (cube pyramid)
 *
 * + MainMenu:
 *
 *     + UI for lobby discovery results to connect
 *
 * + Network:
 *
 *     + Replace delegates in configs and do them public
 *     + Sending messages by groups (queueing)
 *     + All errors fixed
 *     + Objects configs correction
 *     + Network discovery for lobby's
 *     + Falling back system
 *     - (OPTIONAL) Channel management if game will be laggy
 *
 * - UI:
 * 
 *     + (OPTIONAL) Divide console prefab on parts and hide all fields
 * 
 * - AR:
 *
 *     - Plant a mark and move scene to AR
 *     - Hide visible walls on scene
 *
 * - Controls:
 *
 *     - Control system for AR gaming:
 *         - Crosshair
 *         - Some other UI 
 *         - Commands
 *         
*/
