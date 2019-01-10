using System.Collections.Generic;
using Events;
using Events.EventTypes;
using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; }
        public bool Hosting { get; set; }

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;
        private SendMultiplayerMessage _smm;
        private ReceiveMultiplayerMessage _rmm;

        private int _spawnId;
        private int _identityCounter;
        private List<Player> _players;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "MultiplayerManager";
        }
        private void Start()
        {
            _players = new List<Player>();
            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();
            _smm = EventManager.Singleton.GetEvent<SendMultiplayerMessage>();
            _rmm = EventManager.Singleton.GetEvent<ReceiveMultiplayerMessage>();
            _rnm.Subscribe(PollMM);
            _smm.Subscribe(SendMM);
        }
        #endregion

        private void SendMM(AMultiplayerMessage message)
        {
            if (Hosting)
                PollMM(message);
            else
                _snm.Publish(message);
        }
        private void PollMM(AMultiplayerMessage message)
        {
            switch (message.multiplayerMessageType)
            {
                case MultiplayerMessageType.Boop:
                    Debug.Log(" > Boop from multiplayer layer");
                    break;

                case MultiplayerMessageType.Connect:
                    Connect((Connect) message);
                    break;

                case MultiplayerMessageType.Move:
                    Move((Move) message);
                    break;

                case MultiplayerMessageType.RigidbodySynchronization:
                    SynchronizeRigidbody((RBSync) message);
                    break;

                case MultiplayerMessageType.Disconnect:
                    Disconnect((Disconnect) message);
                    break;
            }
        }
        private void Connect(Connect message)
        {
            if (_spawnId > 4) _spawnId = 0;

            var spawn = GameObject.Find(string.Format("SpawnPoint{0}", ++_spawnId)).GetComponent<Transform>();
            var scene = GameObject.Find("Scene").GetComponent<Transform>();

            var playerObject = Instantiate((GameObject)Resources.Load("Game/Player"), scene.transform);
            playerObject.transform.position = spawn.position;
            playerObject.name = string.Format("Player<{0}>", message.PlayerName);

            var playerScript = playerObject.GetComponent<Player>();
            playerScript.playerId = ++_identityCounter;
            playerScript.playerName = message.PlayerName;
            playerScript.playerColor = message.PlayerColor;

            _players.Add(playerScript);
        }
        private void Move(Move message)
        {
        }
        private void SynchronizeRigidbody(RBSync message)
        {
        }
        private void Disconnect(Disconnect message)
        {
            var player = _players.Find(e => e.playerId == message.PlayerId);
            _players.Remove(player);
            Destroy(player);
        }
    }
}