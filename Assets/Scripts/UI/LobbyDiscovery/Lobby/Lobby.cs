using Events;
using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace UI.LobbyDiscovery
{
    public class Lobby : MonoBehaviour
    {
        public Text lobbyNameText;

        private Timer _destroy;
        private string _lobbyName;
        private MessageWrapper _wrapper;

        public void ImmediateStart(MessageWrapper wrapper)
        {
            _wrapper = wrapper;
            _lobbyName = (_wrapper.message as FoundLobby).lobbyName;
            _destroy = gameObject.AddComponent<Timer>();
            _destroy.Duration = 2f;
            _destroy.Discard();
            _destroy.Running = true;
        }
        private void Start()
        {
            name = string.Format("Lobby<{0}>", _lobbyName);
            lobbyNameText.text = _lobbyName;
        }
        private void Update()
        {
            if (_destroy.Elapsed) Destroy(gameObject);
        }

        public void Prolong()
        {
            _destroy.Discard();
        }
        public void Connect()
        {
            EventManager.Singleton.Publish(GameEventType.ConnectToLobby, _wrapper);
        }
    }
}
