using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class Console : MonoBehaviour
    {
        public int maxMessages;
        public GameObject consoleSurface;
        public LinkedList<ConsoleMessage> previousMessages;

        private LinkedList<GameObject> _messages;

        #region MonoBehaviour
        private void Start()
        {
            _messages = new LinkedList<GameObject>();

            if (previousMessages != null)
            {
                foreach (var m in previousMessages)
                {
                    WriteMessage(m);
                }
            }
        }
        #endregion

        public void WriteMessage(ConsoleMessage m)
        {
            if (_messages.Count >= maxMessages)
            {
                Destroy(_messages.First.Value);
                _messages.Remove(_messages.First);
            }

            var newMessageInstance = Instantiate(Resources.Load("UI/Console/Message") as GameObject, consoleSurface.transform);
            var newMessageText = newMessageInstance.GetComponent<Text>();

            newMessageText.text = m.text;
            newMessageText.color = m.color;

            _messages.AddLast(newMessageInstance);
        }
    }
}