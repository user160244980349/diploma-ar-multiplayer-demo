using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Diploma.Network;
using Diploma.Events;
using Diploma.Events.GameEvents;
using Diploma.Scenes;
using Diploma.Multiplayer;

namespace Diploma {

    public class ApplicationManager : MonoBehaviour {

        static ApplicationManager instance = null;

        ButtonClicked buttonClick;

        Client client;
        Host host;

        void Awake () {

            if (instance == null) {
                instance = this;

                // subsystems
                gameObject.AddComponent<EventManager>();
                gameObject.AddComponent<SceneManager>();
                gameObject.AddComponent<NetworkManager>();
                gameObject.AddComponent<MultiplayerManager>();

                client = new Client();
                host = new Host();

                client.Boot();

            } else if (instance == this) {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        public ApplicationManager GetInstance () {
            return instance;
        }

        void Start () {
            buttonClick = EventManager.GetInstance().GetEvent<ButtonClicked>();
            buttonClick.Subscribe(OnButtonClick);

            SceneManager.GetInstance().LoadScene("MainMenu");
        }

        void OnButtonClick (Button button) {

            if (button.name == "Quit")
                Application.Quit();

            if (button.name == "Host") {
                Debug.Log("Booting host");
                host.Boot();
            }

            if (button.name == "Connect") {
                Debug.Log("Booting client");
                client.Connect();
            }

            if (button.name == "Disconnect") {
                client.Disconnect();
            }

            if (button.name == "Send") {
                client.Send();
            }

        }

    }

}
