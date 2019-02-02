using Events;
using UnityEngine;

namespace Scenes
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            gameObject.name = "SceneManager";
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            LoadScene("MainMenu");

            EventManager.Singleton.RegisterListener(GameEventType.Connecting, OnConnecting);
            EventManager.Singleton.RegisterListener(GameEventType.Connected, OnConnected);
            EventManager.Singleton.RegisterListener(GameEventType.LoggingIn, OnLoggingIn);
            EventManager.Singleton.RegisterListener(GameEventType.LoggedIn, OnLoggedIn);
            EventManager.Singleton.RegisterListener(GameEventType.LoggingOut, OnLoggingOut);
            EventManager.Singleton.RegisterListener(GameEventType.LoggedOut, OnLoggedOut);
            EventManager.Singleton.RegisterListener(GameEventType.Disconnected, OnDisconnected);
        }

        private void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        private void OnConnecting(object info)
        {
            LoadScene("Loading");
        }
        private void OnConnected(object info)
        {
            LoadScene("LogIn");
        }
        private void OnLoggingIn(object info)
        {
            LoadScene("Loading");
        }
        private void OnLoggedIn(object info)
        {
            LoadScene("Playground");
        }
        private void OnLoggingOut(object info)
        {
            LoadScene("Loading");
        }
        private void OnLoggedOut(object info)
        {

        }
        private void OnDisconnecting(object info)
        {

        }
        private void OnDisconnected(object info)
        {
            LoadScene("MainMenu");
        }
    }
}