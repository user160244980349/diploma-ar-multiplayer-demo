using Events;
using Events.EventTypes;
using Network;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class PlaygroundManager : MonoBehaviour
    {
        private ButtonClicked _buttonClick;
        private Client _client;
        private GameObject _console;
        private Host _host;
        private GameObject _menu;

        public static PlaygroundManager Instance { get; private set; }

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

            _menu = GameObject.Find("Menu");
            _menu.SetActive(false);
            
            _console = ConsoleManager.Instance.InstantiateOnScene();

            _buttonClick = EventManager.Instance.GetEvent<ButtonClicked>();
            _buttonClick.Subscribe(OnButtonClick);
        }

        private void OnButtonClick(Button button)
        {
            switch (button.name)
            {
                case "Resume":
                    _menu.SetActive(false);
                    break;

                case "Leave":
                    if (_client.Connected) _client.Disconnect();
                    if (_host.Booted) _host.Shutdown();
                    break;

                case "ShowMenu":
                    _menu.SetActive(true);
                    break;

                case "ShowConsole":
                    _console.SetActive(!_console.activeSelf);
                    break;

                case "Send":
                    if (_client.Connected) _client.Send();
                    break;
            }
        }

        private void OnDestroy()
        {
            _buttonClick.Unsubscribe(OnButtonClick);
        }
    }
}