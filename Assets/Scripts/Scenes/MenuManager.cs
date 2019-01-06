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
        private static MenuManager _instance;

        private ButtonClicked _buttonClick;
        private Text _ip;
        private Text _port;

        #region MonoBehaviour
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            ConsoleManager.GetInstance().InstantiateOnScene();

            _ip = GameObject.Find("Ip").GetComponentInChildren<Text>();
            _port = GameObject.Find("Port").GetComponentInChildren<Text>();

            _buttonClick = EventManager.GetInstance().GetEvent<ButtonClicked>();
            _buttonClick.Subscribe(OnButtonClick);
        }
        private void OnDestroy()
        {
            _buttonClick.Unsubscribe(OnButtonClick);
        }
        #endregion

        public static MenuManager GetInstance()
        {
            return _instance;
        }

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

                default:
                    break;
            }
        }
        private void Quitting()
        {
            if (Client.GetInstance().GetState() == ClientState.Ready) Client.GetInstance().Shutdown();
            if (Host.GetInstance().GetState() == HostState.Up) Host.GetInstance().Shutdown();
            Application.Quit();
        }
        private void Hosting()
        {
            if (Host.GetInstance().GetState() == HostState.Down) Host.GetInstance().Boot();
            var cc = new ConnectionConfiguration
            {
                ip = "127.0.0.1",
                port = 8000,
                notificationLevel = 1,
                exceptionConnectionId = 0
            };
            if (Client.GetInstance().GetState() == ClientState.Down) Client.GetInstance().Boot();
            if (Client.GetInstance().GetState() == ClientState.Ready) Client.GetInstance().Connect(cc);
            ApplicationManager.GetInstance().LoadScene("Loading");
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
            if (Client.GetInstance().GetState() == ClientState.Down) Client.GetInstance().Boot();
            if (Client.GetInstance().GetState() == ClientState.Ready) Client.GetInstance().Connect(cc);
            ApplicationManager.GetInstance().LoadScene("Loading");
        }
        #endregion
    }
}
