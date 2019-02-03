using Events;
using Network.Messages;
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
            var wrapper = info as MessageWrapper;
            Lobby lobbyScript;
            if (_lobbys.ContainsKey(wrapper.ip))
            {
                if (!_lobbys.TryGetValue(wrapper.ip, out lobbyScript))
                {
                    _lobbys.Remove(wrapper.ip);
                    return;
                }
                lobbyScript.Prolong();
                return;
            }
            var newLobby = Instantiate(lobbyPrefab, content);
            lobbyScript = newLobby.GetComponent<Lobby>();
            lobbyScript.ImmediateStart(wrapper);
            _lobbys.Add(wrapper.ip, lobbyScript);
        }
    }
}
