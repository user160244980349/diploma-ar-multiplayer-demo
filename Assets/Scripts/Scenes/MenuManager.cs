using Events;
using Events.EventTypes;
using Multiplayer;
using Network;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Singleton { get; private set; }

        private ButtonClicked _buttonClick;
        private Text _ip;
        private Text _port;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
        }
        private void Start()
        {
            ConsoleManager.Singleton.InstantiateConsole();

            _ip = GameObject.Find("Ip").GetComponentInChildren<Text>();
            _port = GameObject.Find("Port").GetComponentInChildren<Text>();

            _buttonClick = EventManager.Singleton.GetEvent<ButtonClicked>();
            _buttonClick.Subscribe(OnButtonClick);
        }
        private void OnDestroy()
        {
            _buttonClick.Unsubscribe(OnButtonClick);
        }
        #endregion

        private void OnButtonClick(Button button)
        {
            switch (button.name)
            {
                case "Quit":
                    Quitting();
                    break;

                case "Host":
                    Hosting();
                    break;

                case "Connect":
                    Connecting();
                    break;
            }
        }
        private void Quitting()
        {
            if (NetworkClient.Singleton.State == ClientState.Ready) NetworkClient.Singleton.Shutdown();
            if (NetworkHost.Singleton.State == HostState.Up) NetworkHost.Singleton.Shutdown();
            MultiplayerManager.Singleton.Hosting = false;
            Application.Quit();
        }
        private void Hosting()
        {
            MultiplayerManager.Singleton.Hosting = true;
            ApplicationManager.Singleton.LoadScene("Loading");

            if (NetworkHost.Singleton.State == HostState.Down) NetworkHost.Singleton.Boot();
        }
        private void Connecting()
        {
            MultiplayerManager.Singleton.Hosting = false;
            ApplicationManager.Singleton.LoadScene("Loading");

            var cc = new ConnectionConfiguration
            {
                ip = "127.0.0.1",
                // ip = "192.168.1.35",
                port = 8000,
                notificationLevel = 1,
                exceptionConnectionId = 0
            };

            if (NetworkClient.Singleton.State == ClientState.Down) NetworkClient.Singleton.Boot();
            if (NetworkClient.Singleton.State == ClientState.Ready) NetworkClient.Singleton.Connect(cc);
        }
    }
}