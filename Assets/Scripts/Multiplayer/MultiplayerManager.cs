using System.Collections.Generic;
using Events;
using Multiplayer.Messages;
using Network;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; }
        public bool Hosting { get; set; }

        private int _spawnId;
        private int _identityCounter;
        private List<LocalPlayer> _players;

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
            _players = new List<LocalPlayer>();

            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageReceived, PollMM);
            EventManager.Singleton.RegisterListener(GameEventType.MultiplayerMessageSend, SendMM);
        }
        #endregion

        private void SendMM(object info)
        {
            if (Hosting)
                PollMM(info);
            else
                EventManager.Singleton.Publish(GameEventType.NetworkMessageSend, info);
        }
        private void PollMM(object info)
        {
            var message = info as AMultiplayerMessage;
            switch (message.multiplayerMessageType)
            {
                case MultiplayerMessageType.Boop:
                    Debug.Log(" > Boop from multiplayer layer");
                    break;

                case MultiplayerMessageType.Connect:
                    Connect(message as LogIn);
                    break;

                case MultiplayerMessageType.Move:
                    Move(message as Move);
                    break;

                case MultiplayerMessageType.RigidbodySynchronization:
                    SynchronizeRigidbody(message as RBSync);
                    break;

                case MultiplayerMessageType.Disconnect:
                    Disconnect(message as LogOut);
                    break;
            }
        }
        private void Connect(LogIn message)
        {
            if (_spawnId > 3) _spawnId = 0;

            var spawn = GameObject.Find(string.Format("SpawnPoint{0}", ++_spawnId)).GetComponent<Transform>();
            var scene = GameObject.Find("Scene").GetComponent<Transform>();

            var playerObject = Instantiate(Resources.Load("Game/Player") as GameObject, scene.transform);
            playerObject.transform.position = spawn.position;
            playerObject.name = string.Format("Player<{0}>", message.PlayerName);

            var playerScript = playerObject.GetComponent<LocalPlayer>();
            playerScript.playerId = ++_identityCounter;
            playerScript.playerName = message.PlayerName;
            playerScript.playerColor = message.PlayerColor;

            _players.Add(playerScript);
            Debug.LogFormat("Player {0} connected '{1}'", playerScript.playerId, playerScript.name);
        }
        private void Move(Move message)
        {
        }
        private void SynchronizeRigidbody(RBSync message)
        {
        }
        private void Disconnect(LogOut message)
        {
            Debug.LogFormat("Player {0} disconnected", message.PlayerId);
            var player = _players.Find(e => e.playerId == message.PlayerId);
        }
    }
}