using Events;
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
        
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
        }
        private void Start()
        {
            ConsoleManager.Singleton.InstantiateConsole();
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
            EventManager.Singleton.Publish(GameEventType.Connecting, null);
            MultiplayerManager.Singleton.Hosting = true;
            NetworkManager.Singleton.SpawnHost();
        }
        private void Connecting()
        {
            EventManager.Singleton.Publish(GameEventType.Connecting, null);
            MultiplayerManager.Singleton.Hosting = false;
            NetworkManager.Singleton.SpawnClient();
        }
    }
}