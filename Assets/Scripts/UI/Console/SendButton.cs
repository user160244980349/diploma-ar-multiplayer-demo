using UnityEngine;
using UnityEngine.UI;

namespace UI.Console {

    public class SendButton : MonoBehaviour {

        public Console          console;
        public InputField       input;
        public Network.Client   client;

        void Start () {

            console = GetComponentInParent<Console>();

            Button button = GetComponent<Button>();
            button.onClick.AddListener(Click);

        }

        public void Click () {

            console.SendMessage(input.text, Color.green);

        }

    }

}