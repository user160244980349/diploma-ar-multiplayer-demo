using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Diploma.Events;
using Diploma.Events.GameEvents;

namespace Diploma.UI {

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