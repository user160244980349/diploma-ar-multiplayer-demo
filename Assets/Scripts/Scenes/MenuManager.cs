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
        private ButtonClicked _buttonClick;
        private Text _ip;
        private Text _port;
        public static MenuManager Singleton { get; private set; }

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

        #region Button events
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
            if (Client.Singleton.State == ClientState.Ready) Client.Singleton.Shutdown();
            if (Host.Singleton.State == HostState.Up) Host.Singleton.Shutdown();
            MultiplayerManager.Singleton.Hosting = false;
            Application.Quit();
        }
        private void Hosting()
        {
            MultiplayerManager.Singleton.Hosting = true;
            ApplicationManager.Singleton.LoadScene("Loading");
            
            if (Host.Singleton.State == HostState.Down) Host.Singleton.Boot();
        }
        private void Connecting()
        {
            MultiplayerManager.Singleton.Hosting = false;
            ApplicationManager.Singleton.LoadScene("Loading");
            
            var cc = new ConnectionConfiguration
            {
                ip = "127.0.0.1",
                port = 8000,
                notificationLevel = 1,
                exceptionConnectionId = 0
            };

            if (Client.Singleton.State == ClientState.Down) Client.Singleton.Boot();
            if (Client.Singleton.State == ClientState.Ready) Client.Singleton.Connect(cc);
        }
        #endregion
    }
}