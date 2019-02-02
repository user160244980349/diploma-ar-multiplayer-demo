using Events;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class PlaygroundManager : MonoBehaviour
    {
        public static PlaygroundManager Singleton { get; private set; }

        private GameObject _menu;

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

            EventManager.Singleton.RegisterListener(GameEventType.ButtonClicked, OnButtonClick);
        }
        private void OnDestroy()
        {
            EventManager.Singleton.UnregisterListener(GameEventType.ButtonClicked, OnButtonClick);
        }

        private void OnButtonClick(object info)
        {
            var button = info as Button;
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

        }
    }
}