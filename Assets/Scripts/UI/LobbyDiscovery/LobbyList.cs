using Events;
using Network.Messages;
using Network.Messages.Wrappers;
using System.Collections.Generic;
using UnityEngine;

namespace UI.LobbyDiscovery
{
    public class LobbyList : MonoBehaviour
    {
        public Transform content;
        public GameObject lobbyPrefab;

        private Dictionary<string, Lobby> _lobbys;

        private void Start()
        {
            _lobbys = new Dictionary<string, Lobby>();

            EventManager.Singleton.Subscribe(GameEventType.FoundLobby, OnLobbyFound);
        }
        private void OnLobbyFound(object info)
        {
            var wrapper = (ReceiveWrapper)info;
            if (_lobbys.ContainsKey(wrapper.ip))
            {
                _lobbys.TryGetValue(wrapper.ip, out Lobby lobbyScript);
                if (lobbyScript == null)
                {
                    _lobbys.Remove(wrapper.ip);
                    return;
                }
                lobbyScript.Prolong();
                return;
            }
            AddLobby(wrapper);
        }
        private void AddLobby(ReceiveWrapper wrapper)
        {
            var newLobby = Instantiate(lobbyPrefab, content);
            var lobbyScript = newLobby.GetComponent<Lobby>();
            lobbyScript.Name = (wrapper.message as FoundLobby).lobbyName;
            lobbyScript.Wrapper = wrapper;
            _lobbys.Add(wrapper.ip, lobbyScript);
            UpdateIds();
        }
        private void UpdateIds()
        {
            var lobbyId = 0;
            foreach (var lobby in _lobbys.Values)
            {
                lobbyId++;
                lobby.Id = lobbyId;
            }
        }
    }
}
