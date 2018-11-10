﻿using System.Collections.Generic;
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

		List<Client> clients;
		List<Host> hosts;

		void Awake () {

			if (instance == null) {
				instance = this;

				// subsystems
				gameObject.AddComponent<EventManager>();
				gameObject.AddComponent<SceneManager>();
				gameObject.AddComponent<NetworkManager>();
				gameObject.AddComponent<MultiplayerManager>();

				clients = new List<Client>();
				hosts = new List<Host>();

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

			if (button.name == "Host")
				hosts.Add(new Host());

			if (button.name == "Connect")
				clients.Add(new Client());

		}

    }

}
