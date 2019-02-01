using Events;
using Multiplayer.Messages;
using UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class LogInManager : MonoBehaviour
    {
        public static LogInManager Singleton { get; private set; }

        private InputField _name;

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
            EventManager.Singleton.RegisterListener(GameEventType.ButtonClicked, OnButtonClick);
            _name = GameObject.Find("PlayerName").GetComponent<InputField>();
        }
        private void OnDestroy()
        {
            EventManager.Singleton.UnregisterListener(GameEventType.ButtonClicked, OnButtonClick);
        }
        #endregion

        private void OnButtonClick(object info)
        {
            var button = info as Button;
            switch (button.name)
            {
                case "LogIn":
                    LogIn();
                    break;
            }
        }
        private void LogIn()
        {
            EventManager.Singleton.Publish(GameEventType.LoggingIn, null);
            EventManager.Singleton.Publish(GameEventType.MultiplayerMessageSend, new LogIn(_name.text, Color.green));
        }
    }
}