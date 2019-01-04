using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class Console : MonoBehaviour
    {
        public GameObject consolePanel;
        public int MaxMessages = 100;
        public GameObject MessageObject;

        private LinkedList<GameObject> messages;

        private void Start()
        {
            messages = new LinkedList<GameObject>();
        }

        public void SendMessage(string text, Color color)
        {
            if (messages.Count >= MaxMessages)
            {
                Destroy(messages.First.Value);
                messages.Remove(messages.First);
            }

            var newMessageInstance = Instantiate(MessageObject, consolePanel.transform);
            var newMessageText = newMessageInstance.GetComponent<Text>();

            newMessageText.color = color;
            newMessageText.text = text;

            messages.AddLast(newMessageInstance);
        }
    }
}