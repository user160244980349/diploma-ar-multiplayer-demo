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
            console.SendMessage(input.text, Color.green);
        }
    }
}