using Events;
using Events.EventTypes;
using Network;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }

        private ButtonClicked _buttonClick;
        private Text _ip;
        private Text _port;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance == this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ConsoleManager.Instance.InstantiateOnScene();

            _ip = GameObject.Find("Ip").GetComponentInChildren<Text>();
            _port = GameObject.Find("Port").GetComponentInChildren<Text>();

            _buttonClick = EventManager.Instance.GetEvent<ButtonClicked>();
            _buttonClick.Subscribe(OnButtonClick);
        }

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
                    
                default:
                    break;
            }
        }

        private void OnDestroy()
        {
            _buttonClick.Unsubscribe(OnButtonClick);
        }

        private void Quitting()
        {
            if (Client.Instance.GetState() == ClientState.Ready) Client.Instance.Shutdown();
            if (Host.Instance.GetState() == HostState.Up) Host.Instance.Shutdown();
            Application.Quit();
        }

        private void Hosting()
        {
            if (Host.Instance.GetState() == HostState.Down) Host.Instance.Boot();
            var cc = new ConnectionConfiguration
            {
                ip = "127.0.0.1",
                port = 8000,
                notificationLevel = 1,
                exceptionConnectionId = 0
            };
            if (Client.Instance.GetState() == ClientState.Down) Client.Instance.Boot();
            if (Client.Instance.GetState() == ClientState.Ready) Client.Instance.Connect(cc);
            ApplicationManager.Instance.LoadScene("Loading");
        }

        private void Connecting()
        {
            var cc = new ConnectionConfiguration
            {
                ip = "127.0.0.1",
                port = 8000,
                notificationLevel = 1,
                exceptionConnectionId = 0
            };
            if (Client.Instance.GetState() == ClientState.Down) Client.Instance.Boot();
            if (Client.Instance.GetState() == ClientState.Ready) Client.Instance.Connect(cc);
            ApplicationManager.Instance.LoadScene("Loading");
        }
    }
}