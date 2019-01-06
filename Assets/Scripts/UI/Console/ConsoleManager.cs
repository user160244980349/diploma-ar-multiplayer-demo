using System.Collections.Generic;
using UnityEngine;

namespace UI.Console
{
    public class ConsoleManager : MonoBehaviour
    {
        public bool WithStackTrace;

        private static ConsoleManager _instance;
        private Console console;
        private int _maxMessages = 200;
        private LinkedList<ConsoleMessage> _messages;

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

        public static ConsoleManager GetInstance()
        {
            return _instance;
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

        public void SendMessage(ConsoleMessage message)
        {
            if (_messages.Count >= _maxMessages)
            {
                _messages.Remove(_messages.First);
            }
            _messages.AddLast(message);

            if (console)
                console.WriteMessage(message);
        }

        public void InstantiateOnScene()
        {
            var canvas = GameObject.Find("Canvas");
            var consolePanel = Instantiate (Resources.Load("UI/Console/Console") as GameObject, canvas.transform);

            console = consolePanel.GetComponent<Console>();
            console.previousMessages = _messages;
            console.maxMessages = _maxMessages;
        }
    }
}
