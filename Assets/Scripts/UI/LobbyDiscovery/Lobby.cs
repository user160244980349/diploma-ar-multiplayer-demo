using Events;
using Network.Messages.Wrappers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.LobbyDiscovery
{
    public class Lobby : MonoBehaviour
    {
        public Text lobbyIdText;
        public Text lobbyNameText;

        public int Id {
            get {
                return _id;
            }
            set {
                _id = value;
                lobbyIdText.text = string.Format("{0}.", value);
            }
        }
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
                if (_name == "") _name = "Лобби без названия";
                lobbyNameText.text = _name;
            }
        }
        public string Ip { get { return Wrapper.ip; } }
        public int Port { get { return Wrapper.port; } }
        public ReceiveWrapper Wrapper { get; set; }

        private int _id;
        private string _name;
        private float _destroyTimer;

        public void Prolong()
        {
            _destroyTimer = 2f;
        }
        public void Connect()
        {
            EventManager.Singleton.Publish(GameEventType.ConnectToLobby, Wrapper);
        }

        private void Start()
        {
            _destroyTimer = 2f;
        }
        private void Update()
        {
            _destroyTimer -= Time.deltaTime;
            if (_destroyTimer < 0) Destroy(gameObject);
        }
    }
}
