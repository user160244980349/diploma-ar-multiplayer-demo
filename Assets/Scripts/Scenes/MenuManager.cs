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
        private ButtonClicked _buttonClick;
        private Client _client;
        private GameObject _console;
        private Host _host;
        private Text _ip;
        private Text _port;

        public static MenuManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            var application = GameObject.Find("Application");
            _client = application.GetComponent<Client>();
            _host = application.GetComponent<Host>();

            _ip = GameObject.Find("Ip").GetComponentInChildren<Text>();
            _port = GameObject.Find("Port").GetComponentInChildren<Text>();

            
            _console = ConsoleManager.Instance.InstantiateOnScene();

            _buttonClick = EventManager.Instance.GetEvent<ButtonClicked>();
            _buttonClick.Subscribe(OnButtonClick);
        }

        private void OnButtonClick(Button button)
        {
            switch (button.name)
            {
                case "ShowConsole":
                    _console.SetActive(!_console.activeSelf);
                    break;

                case "Quit":
                    if (_client.Booted) _client.Shutdown();
                    if (_host.Booted) _host.Shutdown();
                    Application.Quit();
                    break;

                case "Host":
                {
                    Debug.Log("Booting host");
                    if (!_host.Booted) _host.Boot();
                    Debug.Log("Booting client");
                    var cc = new ConnectionConfiguration
                    {
                        ip = "127.0.0.1",
                        port = 8000,
                        notificationLevel = 1,
                        exceptionConnectionId = 0
                    };
                    if (!_client.Booted) _client.Boot();
                    if (!_client.Connected) _client.Connect(cc);
                    break;
                }

                case "Connect":
                {
                    Debug.Log("Booting client");
                    var cc = new ConnectionConfiguration
                    {
                        ip = "127.0.0.1",
                        port = 8000,
                        notificationLevel = 1,
                        exceptionConnectionId = 0
                    };
                    if (!_client.Booted) _client.Boot();
                    if (!_client.Connected) _client.Connect(cc);
                    break;
                }
                    
                default:
                    break;
            }
        }

        private void OnDestroy()
        {
            _buttonClick.Unsubscribe(OnButtonClick);
        }
    }
}