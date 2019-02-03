using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Console
{
    public class ConsoleManager : MonoBehaviour
    {
        public static ConsoleManager Singleton { get; private set; }
        public bool withStackTrace;

        private const int MaxMessages = 200;
        private Console _console;
        private GameObject _consolePrefab;
        private LinkedList<Message> _messages;
        private Queue<Message> _queue;

        public void SendMessage(Message message)
        {
            _queue.Enqueue(message);
        }

        private void Awake()
        {
            name = "ConsoleManager";
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);

            _consolePrefab = Resources.Load("UI/Console/Console") as GameObject;

            _messages = new LinkedList<Message>();
        }
        private void OnEnable()
        {
            _queue = new Queue<Message>();

            Application.logMessageReceivedThreaded += SendLog;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= SendLog;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void Update()
        {
            while (_queue.Count > 0)
            {
                var message = _queue.Dequeue();
                _messages.AddLast(message);
                if (_messages.Count >= MaxMessages) _messages.Remove(_messages.First);
                if (_console != null) _console.WriteMessage(message);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (loadSceneMode != LoadSceneMode.Single) return;
            var canvas = GameObject.Find("Canvas");
            var consolePanel = Instantiate(_consolePrefab, canvas.transform);
            consolePanel.name = "Console";

            _console = consolePanel.GetComponent<Console>();
            _console.Init(MaxMessages, _messages);
        }
        private void SendLog(string condition, string stackTrace, LogType type)
        {
            Message message;

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