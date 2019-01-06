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
        public LinkedList<GameObject> messages;

        private void Start()
        {
            messages = new LinkedList<GameObject>();

            if (previousMessages != null)
            {
                foreach (var m in previousMessages)
                {
                    WriteMessage(m);
                }
            }
        }

        public void WriteMessage(ConsoleMessage m)
        {
            if (messages.Count >= maxMessages)
            {
                Destroy(messages.First.Value);
                messages.Remove(messages.First);
            }

            var newMessageInstance = Instantiate(Resources.Load("UI/Console/Message") as GameObject, consoleSurface.transform);
            var newMessageText = newMessageInstance.GetComponent<Text>();

            newMessageText.text = m.text;
            newMessageText.color = m.color;

            messages.AddLast(newMessageInstance);
        }
    }
}