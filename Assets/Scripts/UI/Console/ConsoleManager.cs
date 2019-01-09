using System.Collections.Generic;
using UnityEngine;

namespace UI.Console
{
    public class ConsoleManager : MonoBehaviour
    {
        private readonly int _maxMessages = 200;
        private Console _console;
        private LinkedList<ConsoleMessage> _messages;
        public bool WithStackTrace;
        public static ConsoleManager Singleton { get; private set; }
        public void InstantiateConsole()
        {
            var canvas = GameObject.Find("Canvas");
            var consolePanel = Instantiate(Resources.Load("UI/Console/Console") as GameObject, canvas.transform);

            _console = consolePanel.GetComponent<Console>();
            _console.previousMessages = _messages;
            _console.maxMessages = _maxMessages;
        }
        public void SendMessage(ConsoleMessage message)
        {
            if (_messages.Count >= _maxMessages) _messages.Remove(_messages.First);
            _messages.AddLast(message);

            if (_console)
                _console.WriteMessage(message);
        }
        private void SendLog(string condition, string stackTrace, LogType type)
        {
            ConsoleMessage message;

            if (WithStackTrace && type != LogType.Log)
                condition = string.Format("Message: {0}\nStackTrace: {1}", condition, stackTrace);

            message.text = condition;

            switch (type)
            {
                case LogType.Log:
                    message.color = Color.blue;
                    break;

                case LogType.Warning:
                    message.color = Color.yellow;
                    break;

                case LogType.Error:
                    message.color = Color.red;
                    break;

                case LogType.Exception:
                    message.color = Color.red;
                    break;

                case LogType.Assert:
                    message.color = Color.red;
                    break;

                default:
                    message.color = Color.white;
                    break;
            }

            SendMessage(message);
        }

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            _messages = new LinkedList<ConsoleMessage>();
        }
        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += SendLog;
        }
        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= SendLog;
        }
        #endregion
    }
}