using Events;
using UnityEngine;

namespace Scenes
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Singleton { get; private set; }

        private void Awake()
        {
            name = "SceneManager";
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            // Button triggers
            EventManager.Singleton.Subscribe(GameEventType.BecomeHost, OnBecomeHost);
            EventManager.Singleton.Subscribe(GameEventType.CreateLobby, OnCreateLobby);
            EventManager.Singleton.Subscribe(GameEventType.BecomeClient, OnBecomeClient);
            EventManager.Singleton.Subscribe(GameEventType.ConnectToLobby, OnConnectToLobby);
            EventManager.Singleton.Subscribe(GameEventType.LogIntoLobby, OnLogIntoLobby);
            EventManager.Singleton.Subscribe(GameEventType.LoggingIn, OnLoggingIn);
            EventManager.Singleton.Subscribe(GameEventType.LoggedIn, OnLoggedIn);
            EventManager.Singleton.Subscribe(GameEventType.LogOutLobby, OnLogOutLobby);
            EventManager.Singleton.Subscribe(GameEventType.LoggingOut, OnLoggingOut);
            EventManager.Singleton.Subscribe(GameEventType.LoggedOut, OnLoggedOut);
            EventManager.Singleton.Subscribe(GameEventType.ExitToMainMenu, OnExitToMainMenu);

            // Host events
            EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.DestroyHost, OnDestroyHost);

            // Client events
            EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
            EventManager.Singleton.Subscribe(GameEventType.ConnectedToHost, OnConnectedToHost);
            EventManager.Singleton.Subscribe(GameEventType.DisconnectedFromHost, OnDisconnectedFromHost);

            LoadScene("MainMenu");
        }

        // Button triggers
        private void OnBecomeHost(object info)
        {
            EventManager.Singleton.Publish(GameEventType.StartHost, null);
        }
        private void OnCreateLobby(object info)
        {
            EventManager.Singleton.Publish(GameEventType.StartLobbyBroadcast, info);
            LoadScene("LogIn");
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
            EventManager.Singleton.Publish(GameEventType.LoggingIn, null);
            EventManager.Singleton.Publish(GameEventType.StopLobbyBroadcast, null);
            LoadScene("Loading");
        }
        private void OnLoggingIn(object info)
        {
            EventManager.Singleton.Publish(GameEventType.LoggedIn, null);
            LoadScene("Loading");
        }
        private void OnLoggedIn(object info)
        {
            LoadScene("Playground");
        }
        private void OnLogOutLobby(object info)
        {
            EventManager.Singleton.Publish(GameEventType.LoggingOut, null);
            LoadScene("Loading");
        }
        private void OnLoggingOut(object info)
        {
            EventManager.Singleton.Publish(GameEventType.LoggedOut, null);
        }
        private void OnLoggedOut(object info)
        {
            EventManager.Singleton.Publish(GameEventType.DestroyClient, null);
            EventManager.Singleton.Publish(GameEventType.DestroyHost, null);
        }
        private void OnExitToMainMenu(object info)
        {
            EventManager.Singleton.Publish(GameEventType.StopLobbyBroadcast, null);
            EventManager.Singleton.Publish(GameEventType.DestroyClient, null);
            EventManager.Singleton.Publish(GameEventType.DestroyHost, null);
        }

        // Host events
        private void OnHostStarted(object info)
        {
            LoadScene("LobbyCreation");
        }
        private void OnDestroyHost(object info)
        {
            LoadScene("MainMenu");
        }

        // Client events
        private void OnClientStarted(object info)
        {
            LoadScene("LobbyDiscovery");
        }
        private void OnConnectedToHost(object info)
        {
            LoadScene("LogIn");
        }
        private void OnDisconnectedFromHost(object info)
        {
            EventManager.Singleton.Publish(GameEventType.DestroyClient, null);
            LoadScene("MainMenu");
        }

        private void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}