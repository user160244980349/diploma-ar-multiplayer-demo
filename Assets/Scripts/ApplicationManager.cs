using Events;
using Multiplayer;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager Singleton { get; private set; }

    private void Awake()
    {
        name = "ApplicationManager";
        if (Singleton == null)
            Singleton = this;
        else if (Singleton == this) Destroy(gameObject);

        var _eventManager = Resources.Load("Managers/EventManager") as GameObject;
        var _networkManager = Resources.Load("Managers/NetworkManager") as GameObject;
        var _multiplayerManager = Resources.Load("Managers/MultiplayerManager") as GameObject;
        var _consoleManager = Resources.Load("Managers/ConsoleManager") as GameObject;
        var _sceneManager = Resources.Load("Managers/SceneManager") as GameObject;

        DontDestroyOnLoad(gameObject);

        Instantiate(_eventManager);
        Instantiate(_networkManager);
        Instantiate(_multiplayerManager);
        Instantiate(_consoleManager);
        Instantiate(_sceneManager);
    }
    private void Start()
    {
        EventManager.Singleton.Subscribe(GameEventType.ResetMultiplayerManager, ResetMultiplayerManager);
    }
    private void OnDestroy()
    {
        EventManager.Singleton.Unsubscribe(GameEventType.ResetMultiplayerManager, ResetMultiplayerManager);
    }

    private void ResetMultiplayerManager(object info)
    {
        Destroy(MultiplayerManager.Singleton.gameObject);
        var _multiplayerManager = Resources.Load("Managers/MultiplayerManager") as GameObject;
        Instantiate(_multiplayerManager);
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
 * - Multiplayer:
 * 
 *     + Messages:
 *         + Connection
 *         + Disconnection
 *         + Serialize/Deserialize in formatter
 *         + Smth with message structure (PlayerID add)
 *     + Player identification via ID
 *     + Player spawn
 *     - MultiplayerManager Serialization/Deserialization
 *     - Synchronization logic
 *     - Divide host and client logic by parts
 *     - (OPTIONAL) Smoother interpolation ways 
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
 * # Thoughts
 * 
 *  RBSync by script on object
 *  Commands by script on player
 *  + Disconnect command use is problemmatic
 *  + Think about wait disconnection on update cycle
 * 
*/
