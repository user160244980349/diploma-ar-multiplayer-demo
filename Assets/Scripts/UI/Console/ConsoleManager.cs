using System.Collections.Generic;
using UnityEngine;

namespace UI.Console
{
    public class ConsoleManager : MonoBehaviour
    {
        public static ConsoleManager Singleton { get; private set; }
        public bool withStackTrace;

        private const int MaxMessages = 200;
        private Console _console;
        private LinkedList<ConsoleMessage> _messages;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            gameObject.name = "ConsoleManager";

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

        public void InstantiateConsole()
        {
            var canvas = GameObject.Find("Canvas");
            var consolePanel = Instantiate(Resources.Load("UI/Console/Console") as GameObject, canvas.transform);

            _console = consolePanel.GetComponent<Console>();
            _console.previousMessages = _messages;
            _console.maxMessages = MaxMessages;
        }
        public void SendMessage(ConsoleMessage message)
        {
            if (_messages.Count >= MaxMessages) _messages.Remove(_messages.First);
            _messages.AddLast(message);

            if (_console != null)
                if (_console.Started)
                    _console.WriteMessage(message);
        }
        private void SendLog(string condition, string stackTrace, LogType type)
        {
            ConsoleMessage message;

            if (withStackTrace && stackTrace.Length > 0 && type != LogType.Log)
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
    }
}