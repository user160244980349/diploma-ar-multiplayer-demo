using System.Collections.Generic;
using Events;
using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; }
        public bool Hosting { get; set; }

        //private int _spawnId;
        //private int _identityCounter;
        //private Dictionary<int, Player> _players;
        //private Dictionary<int, RBSynchronizator> _objects;

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
            //_players = new Dictionary<int, Player>();
            //_objects = new Dictionary<int, RBSynchronizator>();

            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageReceived, PollMM);
            EventManager.Singleton.RegisterListener(GameEventType.MultiplayerMessageSend, SendMM);
        }
        #endregion

        private void SendMM(object info)
        {
            if (Hosting)
            {
                PollMM(info);
            }
            else
            {
                EventManager.Singleton.Publish(GameEventType.NetworkMessageSend, info);
            }
        }
        private void PollMM(object info)
        {
            var message = info as AMultiplayerMessage;
            switch (message.multiplayerMessageType)
            {
                case MultiplayerMessageType.LogIn:
                    LogIn(message as LogIn);
                    break;

                case MultiplayerMessageType.LoggedIn:
                    LoggedIn(message as LoggedIn);
                    break;

                case MultiplayerMessageType.Move:
                    Move(message as Move);
                    break;

                case MultiplayerMessageType.RigidbodySynchronization:
                    SynchronizeRigidbody(message as RBSync);
                    break;

                case MultiplayerMessageType.LogOut:
                    LogOut(message as LogOut);
                    break;
            }
        }

        private void LogIn(LogIn message)
        {
            if (Hosting)
            {
                EventManager.Singleton.Publish(GameEventType.NetworkMessageSend, new LoggedIn(1));
            }
            else
            {
                EventManager.Singleton.Publish(GameEventType.LoggedIn, new LoggedIn(1));
            }
            //if (_spawnId > 3) _spawnId = 0;

            //var spawn = GameObject.Find(string.Format("SpawnPoint{0}", ++_spawnId)).GetComponent<Transform>();
            //var scene = GameObject.Find("Scene").GetComponent<Transform>();

            //var playerObject = Instantiate(Resources.Load("Game/Player") as GameObject, scene.transform);
            //playerObject.transform.position = spawn.position;
            //playerObject.name = string.Format("Player<{0}>", message.PlayerName);

            //var playerScript = playerObject.GetComponent<Player>();
            //playerScript.playerId = ++_identityCounter;
            //playerScript.playerName = message.PlayerName;
            //playerScript.playerColor = message.PlayerColor;

            //_players.Add(_players.Count, playerScript);
            //Debug.LogFormat("Player {0} logged in as '{1}'", playerScript.playerId, playerScript.name);
        }
        private void LoggedIn(LoggedIn message)
        {
            EventManager.Singleton.Publish(GameEventType.LoggedIn, new LoggedIn(1));
        }
        private void Move(Move message)
        {
            //_players.TryGetValue(message.PlayerId, out Player player);
            //Debug.LogFormat("Player {0} move", message.PlayerId);
        }
        private void SynchronizeRigidbody(RBSync message)
        {
            //_objects.TryGetValue(message.ObjectId, out RBSynchronizator rbobject);
            //Debug.LogFormat("Object {0} sync rigidbody", message.ObjectId);
        }
        private void LogOut(LogOut message)
        {
            //Debug.LogFormat("Player {0} logged out", message.PlayerId);
            //_players.TryGetValue(message.PlayerId, out Player player);
        }
    }
}