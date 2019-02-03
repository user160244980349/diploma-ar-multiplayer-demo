using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class Console : MonoBehaviour
    {
        public GameObject content;
        public GameObject message;

        private int _maxMessages;
        private LinkedList<GameObject> _messages;
        private LinkedList<Message> _previousMessages;

        private void Start()
        {
            _messages = new LinkedList<GameObject>();
            message = Resources.Load("UI/Console/Message") as GameObject;

            foreach (var m in _previousMessages)
                WriteMessage(m);
        }
        public void Init(int maxMessages, LinkedList<Message> previousMessages)
        {
            _maxMessages = maxMessages;
            _previousMessages = previousMessages;
        }
        public void WriteMessage(Message m)
        {
            var newMessageInstance = Instantiate(message, content.transform);
            var newMessageText = newMessageInstance.GetComponent<Text>();

            newMessageText.text = m.text;
            newMessageText.color = m.color;

            _messages.AddLast(newMessageInstance);

            if (_messages.Count >= _maxMessages)
            {
                Destroy(_messages.First.Value);
                _messages.Remove(_messages.First);
            }
        }
    }
}