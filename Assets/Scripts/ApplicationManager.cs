﻿using System.Collections;
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

        var  _eventManager = (GameObject)Resources.Load("Managers/EventManager");
        var  _networkManager = (GameObject)Resources.Load("Managers/NetworkManager");
        var  _multiplayerManager = (GameObject)Resources.Load("Managers/MultiplayerManager");
        var  _consoleManager = (GameObject)Resources.Load("Managers/ConsoleManager");

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
 * - Event based interfaces
 * 
 * - Multiplayer:
 * 
 *     - Messages:
 *         + Connection
 *         + Disconnection
 *         - Smth with message structure (PlayerID add)
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
 *     + Sending messages by groups (queueing)
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
 * # Thoughts
 *  RBSync by script on object
 *  Commands by script on player
 *  Disconnect command use is problemmatic
 *  Think about wait disconnection on update cycle
 * 
*/
