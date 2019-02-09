using Events;
using Multiplayer;
using System.Collections.Generic;
using UnityEngine;

namespace UI.WaitPlayersOrStart
{
    public class PlayersList : MonoBehaviour
    {
        public Transform content;
        public GameObject playerPrefab;

        private Dictionary<int, Player> _players;

        private void Start()
        {
            _players = new Dictionary<int, Player>();

            EventManager.Singleton.Subscribe(GameEventType.PlayerRegistered, OnPlayerRegistered);
            EventManager.Singleton.Subscribe(GameEventType.PlayerUnregistered, OnPlayerUnregistered);
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Unsubscribe(GameEventType.PlayerRegistered, OnPlayerRegistered);
            EventManager.Singleton.Unsubscribe(GameEventType.PlayerUnregistered, OnPlayerUnregistered);
        }
        private void OnPlayerRegistered(object info)
        {
            var playerModel = info as PlayerModel;
            var playerBox = Instantiate(playerPrefab, content);
            var playerScript = playerBox.GetComponent<Player>();
            playerScript.playerName = playerModel.playerName;
            _players.Add(playerModel.playerId, playerScript);
        }
        private void OnPlayerUnregistered(object info)
        {
            int id = (int)info;
            if (!_players.ContainsKey(id)) return;
            _players.TryGetValue(id, out Player player);
            _players.Remove(id);
            Destroy(player.gameObject);
        }
    }
}
