using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Diploma.UI {

    public class Console : MonoBehaviour {

        public GameObject   MessageObject;
        public int          MaxMessages = 100;
        public GameObject   consolePanel;

        LinkedList<GameObject>  messages;

        void Start () {

            messages = new LinkedList<GameObject>();

        }

        public void SendMessage (string text, Color color) {

            if (messages.Count >= MaxMessages) {
                Destroy(messages.First.Value);
                messages.Remove(messages.First);
            }

            GameObject newMessageInstance   = Instantiate(MessageObject, consolePanel.transform);
            Text newMessageText             = newMessageInstance.GetComponent<Text>();

            newMessageText.color = color;
            newMessageText.text = text;

            messages.AddLast(newMessageInstance);

        }

    }

}