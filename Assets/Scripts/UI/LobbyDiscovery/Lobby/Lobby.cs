using Events;
using Network.Messages;
using Network.Messages.Wrappers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.LobbyDiscovery
{
    public class Lobby : MonoBehaviour
    {
        public int lobbyId;
        public string lobbyName;
        public Text lobbyIdText;
        public Text lobbyNameText;

        private ReceiveWrapper _wrapper;
        private float _destroyTimer;

        public void ImmediateStart(ReceiveWrapper wrapper)
        {
            _destroyTimer = 2f;
            _wrapper = wrapper;
        }
        private void Start()
        {
            name = string.Format("Lobby<{0}>", lobbyName);
            lobbyIdText.text = string.Format("{0}.", lobbyId);
            if (lobbyName == "") lobbyName = string.Format("Лобби без названия");
            lobbyNameText.text = lobbyName;
        }
        private void Update()
        {
            _destroyTimer -= Time.deltaTime;
            if (_destroyTimer < 0) Destroy(gameObject);
        }

        public void Prolong()
        {
            _destroyTimer = 2f;
        }
        public void Connect()
        {
            EventManager.Singleton.Publish(GameEventType.ConnectToLobby, _wrapper);
        }
    }
}
