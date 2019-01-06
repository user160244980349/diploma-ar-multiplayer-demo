using Events;
using Events.EventTypes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class SendButton : MonoBehaviour
    {
        public Console console;
        public InputField input;

        private void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(Click);
        }

        private void Click()
        {
            ConsoleMessage m;
            m.text = input.text;
            m.color = Color.green;
            ConsoleManager.GetInstance().SendMessage(m);
        }
    }
}
