using Events;
using Events.EventTypes;
using Network;
using Network.Messages;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class PlaygroundManager : MonoBehaviour
    {
        private static PlaygroundManager _instance;

        private ButtonClicked _buttonClick;
        private GameObject _menu;

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

            _menu = GameObject.Find("Menu");
            _menu.SetActive(false);

            _buttonClick = EventManager.GetInstance().GetEvent<ButtonClicked>();
            _buttonClick.Subscribe(OnButtonClick);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Client.GetInstance().Send(new Beep("qq"));
            } 
        }
        private void OnDestroy()
        {
            _buttonClick.Unsubscribe(OnButtonClick);
        }
        #endregion

        public static PlaygroundManager GetInstance()
        {
            return _instance;
        }

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
            Client.GetInstance().Disconnect();
            if (Host.GetInstance().GetState() == HostState.Up)
            {
                Host.GetInstance().Shutdown();
            }
            ApplicationManager.GetInstance().LoadScene("Loading");
        }
        #endregion
    }
}
