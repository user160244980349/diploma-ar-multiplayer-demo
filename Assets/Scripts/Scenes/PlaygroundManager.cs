using Events;
using Events.EventTypes;
using Network;
using System.Text;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class PlaygroundManager : MonoBehaviour
    {
        public static PlaygroundManager Instance { get; private set; }

        private ButtonClicked _buttonClick;
        private GameObject _menu;

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

            _menu = GameObject.Find("Menu");
            _menu.SetActive(false);

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
                    Leave();
                    break;

                case "ShowMenu":
                    _menu.SetActive(true);
                    break;
            }
        }

        private void OnDestroy()
        {
            _buttonClick.Unsubscribe(OnButtonClick);
        }

        private void Leave()
        {
            Client.Instance.Disconnect();
            if (Host.Instance.GetState() == HostState.Up)
            {
                Host.Instance.Shutdown();
            }
            ApplicationManager.Instance.LoadScene("Loading");
        }
    }
}