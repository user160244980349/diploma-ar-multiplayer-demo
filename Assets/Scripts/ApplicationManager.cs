using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager Singleton { get; private set; }

    #region MonoBehaviour
    private void Awake()
    {
        if (Singleton == null)
            Singleton = this;
        else if (Singleton == this) Destroy(gameObject);

        var  _eventManager = Resources.Load("Managers/EventManager") as GameObject;
        var  _networkManager = Resources.Load("Managers/NetworkManager") as GameObject;
        var  _multiplayerManager = Resources.Load("Managers/MultiplayerManager") as GameObject;
        var  _consoleManager = Resources.Load("Managers/ConsoleManager") as GameObject;

        DontDestroyOnLoad(gameObject);

        Instantiate(_eventManager);
        Instantiate(_networkManager);
        Instantiate(_multiplayerManager);
        Instantiate(_consoleManager);
    }
    private void Start()
    {
        LoadScene("MainMenu");
    }
    #endregion

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}

/**
 *
 * TODO:
 * 
 * + Refactor
 * 
 * - Event based interfaces
 * 
 * - Multiplayer:
 * 
 *     - Messages:
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
 * - MainMenu:
 *
 *     - UI for lobby discovery results to connect
 *
 * - Network:
 *
 *     x Replace delegates in configs and do them public
 *     + Sending messages by groups (queueing)
 *     + Almost all errors fixed
 *     - Objects configs correction
 *     - Network discovery for lobby's
 *     - Falling back system
 *     - (OPTIONAL) Channel management if game will be laggy
 *
 * - UI:
 * 
 *     - (OPTIONAL) Divide console prefab on parts and hide all fields
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
