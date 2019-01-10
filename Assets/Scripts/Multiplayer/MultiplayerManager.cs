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

        private MultiplayerMessageReady _mmr;
        private List<Player> _players;
        private ReceivedMultiplayerMessage _rmm;

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
            _mmr = EventManager.Singleton.GetEvent<MultiplayerMessageReady>();
            _rmm = EventManager.Singleton.GetEvent<ReceivedMultiplayerMessage>();
            _rmm.Subscribe(PollMessage);
        }
        #endregion

        public void DeployMessage(AMultiplayerMessage message)
        {
            _mmr.Publish(message);
        }
        public void PollMessage(AMultiplayerMessage message)
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
                    SynchronizeRigidbody((RigidbodySynchronization) message);
                    break;

                case MultiplayerMessageType.Disconnect:
                    Disconnect((Disconnect) message);
                    break;
            }
        }
        private void Connect(AMultiplayerMessage message)
        {
        }
        private void Move(AMultiplayerMessage message)
        {
        }
        private void SynchronizeRigidbody(AMultiplayerMessage message)
        {
        }
        private void Disconnect(AMultiplayerMessage message)
        {
        }

    }
}