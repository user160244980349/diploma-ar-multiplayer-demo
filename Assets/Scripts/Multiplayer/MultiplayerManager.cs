using Network;
using Multiplayer.Messages;
using UnityEngine;
using Events.EventTypes;
using Events;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        private static MultiplayerManager _instance;

        private MultiplayerMessageReady _mmr;
        private ReceivedMultiplayerMessage _rmm;

        #region MonoBehaviour
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            _mmr = EventManager.GetInstance().GetEvent<MultiplayerMessageReady>();
            _rmm = EventManager.GetInstance().GetEvent<ReceivedMultiplayerMessage>();
            _rmm.Subscribe(PullMessage);
        }
        private void Update()
        {
        }
        #endregion

        public static MultiplayerManager GetInstance()
        {
            return _instance;
        }

        public void DeployMessage(AMultiplayerMessage message)
        {
            _mmr.Publish(message);
        }
        public void PullMessage(AMultiplayerMessage message)
        {
            var player = GameObject.Find("Player");
            player = GameObject.Find("Player");

            switch (message.multiplayerMessageType)
            {
                case MultiplayerMessageType.Beep:
                    Debug.Log(" > Boop from multiplayer layer");
                    break;

                case MultiplayerMessageType.Move:
                    var rb = player.GetComponent<Rigidbody>();
                    rb.AddForce(((Move)message).GetMove());
                    break;

                case MultiplayerMessageType.TransformSynchronization:
                    var p = player.GetComponent<Player>();
                    p.UpdateTransform((TransformSynchronization)message);
                    break;

                default:
                    break;
            }
        }
    }
}
