﻿using Events;
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
            Lobby lobbyScript;
            if (_lobbys.ContainsKey(wrapper.ip))
            {
                _lobbys.TryGetValue(wrapper.ip, out lobbyScript);
                if (lobbyScript == null)
                {
                    _lobbys.Remove(wrapper.ip);
                    return;
                }
                lobbyScript.Prolong();
                return;
            }
            Debug.LogFormat("NEW LOBBY {0}", wrapper.ip);
            var newLobby = Instantiate(lobbyPrefab, content);
            lobbyScript = newLobby.GetComponent<Lobby>();
            _lobbys.Add(wrapper.ip, lobbyScript);
            lobbyScript.lobbyId = _lobbys.Count;
            lobbyScript.lobbyName = (wrapper.message as FoundLobby).lobbyName;
            lobbyScript.ImmediateStart(wrapper);
        }
    }
}
