using System.Collections.Generic;
using UnityEngine;

namespace UI.Console
{
    public class ConsoleManager : MonoBehaviour
    {
        public bool WithStackTrace;

        private static ConsoleManager _instance;

        private int _maxMessages = 200;
        private Console _console;
        private LinkedList<ConsoleMessage> _messages;

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

        public static ConsoleManager GetInstance()
        {
            return _instance;
        }
        public void InstantiateOnScene()
        {
            var canvas = GameObject.Find("Canvas");
            var consolePanel = Instantiate(Resources.Load("UI/Console/Console") as GameObject, canvas.transform);

            _console = consolePanel.GetComponent<Console>();
            _console.previousMessages = _messages;
            _console.maxMessages = _maxMessages;
        }
        public void SendMessage(ConsoleMessage message)
        {
            if (_messages.Count >= _maxMessages)
            {
                _messages.Remove(_messages.First);
            }
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
    }
}
