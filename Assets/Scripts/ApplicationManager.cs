using System.Collections;
using Events.EventTypes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager Singleton { get; private set; }

    private ButtonClicked _buttonClick;

    #region MonoBehaviour
    private void Awake()
    {
        if (Singleton == null)
            Singleton = this;
        else if (Singleton == this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        Instantiate(Resources.Load("Managers/EventManager"));
        Instantiate(Resources.Load("Managers/NetworkManager"));
        Instantiate(Resources.Load("Managers/MultiplayerManager"));
        Instantiate(Resources.Load("Managers/ConsoleManager"));
        Instantiate(Resources.Load("Networking/NetworkHost"));
        Instantiate(Resources.Load("Networking/NetworkClient"));
    }
    private void Start()
    {
        LoadScene("Loading");
        DelayedLoadScene("MainMenu", 0.25f);
    }
    #endregion

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public void DelayedLoadScene(string sceneName, float time)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, time));
    }
    private IEnumerator LoadSceneCoroutine(string sceneName, float time)
    {
        yield return new WaitForSeconds(time);
        LoadScene(sceneName);
    }
}

/**
 *
 * TODO:
 * 
 * + Refactor
 * 
 * - Multiplayer:
 * 
 *     - Messages:
 *         + Connection
 *         + Disconnection
 *         - Smth with message structure (PlayerID add)
 *     - Player identification via ID
 *     - Player spawn
 *     - Synchronization logic
 *     - Divide host and client logic by parts
 *     - (OPTIONAL) Smoother interpolation ways 
 *
 * - Playground:
 *
 *     - Non-controllable physical objects (cube pyramid)
 *
 * - MainMenu:
 *
 *     - UI for lobby discovery results to connect
 *
 * - Network:
 *
 *     - Sending messages by groups (queueing)
 *     - Network discovery for lobby's
 *     - Falling back system
 *     - (OPTIONAL)Channel management if game will be laggy
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