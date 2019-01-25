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

        private GameObject _hostObject;
        private GameObject _clientObject;

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

            _hostObject = (GameObject)Resources.Load("Networking/NetworkHost");
            _clientObject = (GameObject)Resources.Load("Networking/NetworkClient");

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
            Application.Quit();
        }
        private void Hosting()
        {
            ApplicationManager.Singleton.LoadScene("Loading");
            MultiplayerManager.Singleton.Hosting = true;
            NetworkManager.Singleton.SpawnHost();
        }
        private void Connecting()
        {
            ApplicationManager.Singleton.LoadScene("Loading");
            MultiplayerManager.Singleton.Hosting = false;
            NetworkManager.Singleton.SpawnClient();
        }
    }
}