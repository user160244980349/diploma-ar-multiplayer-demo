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
        private GameObject _menu;

        public static PlaygroundManager Singleton { get; private set; }

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
        private void Leave()
        {
            Debug.Log("Leaving");
            ApplicationManager.Singleton.LoadScene("Loading");

            if (Host.Singleton.State == HostState.Up)
                Host.Singleton.Shutdown();
            else
                Client.Singleton.Disconnect();
        }
        #endregion
    }
}