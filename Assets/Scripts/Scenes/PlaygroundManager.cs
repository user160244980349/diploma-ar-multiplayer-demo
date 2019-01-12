using Events;
using Events.EventTypes;
using Multiplayer.Messages;
using Network;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class PlaygroundManager : MonoBehaviour
    {
        public static PlaygroundManager Singleton { get; private set; }

        private SendMultiplayerMessage _smm;
        private ButtonClicked _buttonClick;
        private GameObject _menu;

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

            _menu = GameObject.Find("Menu");
            _menu.SetActive(false);

            _smm = EventManager.Singleton.GetEvent<SendMultiplayerMessage>();
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
                case "Resume":
                    _menu.SetActive(false);
                    break;

                case "Connect":
                    Connect();
                    break;

                case "Leave":
                    Leave();
                    break;

                case "ShowMenu":
                    _menu.SetActive(true);
                    break;
            }
        }
        private void Connect()
        {
            var playerName = GameObject.Find("PlayerName").GetComponent<InputField>();
            _smm.Publish(new Connect(playerName.text, Color.blue));
            Destroy(GameObject.Find("ConnectDialog"));
        }
        private void Leave()
        {
            _smm.Publish(new Disconnect(2));
            ApplicationManager.Singleton.LoadScene("MainMenu");
        }
    }
}