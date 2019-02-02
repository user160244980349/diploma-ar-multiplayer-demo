using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class Console : MonoBehaviour
    {
        public bool Started { get; private set; }

        public GameObject consoleSurface;
        public int maxMessages;
        public LinkedList<ConsoleMessage> previousMessages;

        private LinkedList<GameObject> _messages;
        private GameObject _messageObject;

        private void Start()
        {
            _messages = new LinkedList<GameObject>();
            _messageObject = Resources.Load("UI/Console/Message") as GameObject;

            if (previousMessages != null)
                foreach (var m in previousMessages)
                    WriteMessage(m);

            Started = true;
        }

        public void WriteMessage(ConsoleMessage m)
        {
            if (_messages.Count >= maxMessages)
            {
                Destroy(_messages.First.Value);
                _messages.Remove(_messages.First);
            }

            var newMessageInstance = Instantiate(_messageObject, consoleSurface.transform);

            var newMessageText = newMessageInstance.GetComponent<Text>();

            newMessageText.text = m.text;
            newMessageText.color = m.color;

            _messages.AddLast(newMessageInstance);
        }
    }
}