using Events;
using Network.Messages;
using Network.Messages.Wrappers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.LobbyDiscovery
{
    public class Lobby : MonoBehaviour
    {
        public Text lobbyNameText;

        private string _lobbyName;
        private ReceiveWrapper _wrapper;
        private float _destroyTimer;

        public void ImmediateStart(ReceiveWrapper wrapper)
        {
            _wrapper = wrapper;
            _lobbyName = (_wrapper.message as FoundLobby).lobbyName;
        }
        private void Start()
        {
            name = string.Format("Lobby<{0}>", _lobbyName);
            lobbyNameText.text = _lobbyName;
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
