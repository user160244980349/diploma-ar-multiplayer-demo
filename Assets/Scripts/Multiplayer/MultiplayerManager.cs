using Events;
using Events.EventTypes;
using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        private MultiplayerMessageReady _mmr;
        private ReceivedMultiplayerMessage _rmm;

        public static MultiplayerManager Singleton { get; private set; }
        public bool Hosting { get; set; }

        public void DeployMessage(AMultiplayerMessage message)
        {
            var player = GameObject.Find("Player");
            var p = player.GetComponent<Player>();
            var rb = player.GetComponent<Rigidbody>();

            if (Hosting)
            {
                rb.isKinematic = false;

                if (message.multiplayerMessageType != MultiplayerMessageType.RigidbodySynchronization)
                    PullMessage(message);

                if (message.multiplayerMessageType == MultiplayerMessageType.RigidbodySynchronization)
                    _mmr.Publish(message);
            }
            else
            {
                if (message.multiplayerMessageType != MultiplayerMessageType.RigidbodySynchronization)
                    _mmr.Publish(message);
            }
        }
        public void PullMessage(AMultiplayerMessage message)
        {
            var player = GameObject.Find("Player");
            switch (message.multiplayerMessageType)
            {
                case MultiplayerMessageType.Boop:
                    Debug.Log(" > Boop from multiplayer layer");
                    break;
                case MultiplayerMessageType.Connect:
                    
                    break;

                case MultiplayerMessageType.Move:

                    if (Hosting)
                    {
                        var rb = player.GetComponent<Rigidbody>();
                        var move = (Move) message;
                        rb.AddForce(move.Vector);
                    }

                    break;

                case MultiplayerMessageType.RigidbodySynchronization:

                    if (!Hosting)
                    {
                        var p = player.GetComponent<Player>();
                        var rigidbodySynchronization = (RigidbodySynchronization) message;
                        p.SynchronizeRigidbody(rigidbodySynchronization);
                    }

                    break;
            }
        }

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            _mmr = EventManager.Singleton.GetEvent<MultiplayerMessageReady>();
            _rmm = EventManager.Singleton.GetEvent<ReceivedMultiplayerMessage>();
            _rmm.Subscribe(PullMessage);
        }
        #endregion
    }
}