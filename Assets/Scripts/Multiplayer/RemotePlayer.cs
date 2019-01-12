using Events;
using Events.EventTypes;
using Multiplayer.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer
{
    public class RemotePlayer : MonoBehaviour
    {
        public int playerId;
        public string playerName;
        public Color playerColor;

        private SendMultiplayerMessage _smm;

        #region MonoBehaviour
        private void Start()
        {
            _smm = EventManager.Singleton.GetEvent<SendMultiplayerMessage>();
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                var v = -Vector3.back * 100;
                _smm.Publish(new Move(v));
            }

            if (Input.GetKey(KeyCode.A))
            {
                var v = -Vector3.right * 100;
                _smm.Publish(new Move(v));
            }

            if (Input.GetKey(KeyCode.S))
            {
                var v = Vector3.back * 100;
                _smm.Publish(new Move(v));
            }

            if (Input.GetKey(KeyCode.D))
            {
                var v = Vector3.right * 100;
                _smm.Publish(new Move(v));
            }

            if (Input.GetKey(KeyCode.Space))
            {
                var v = Vector3.up * 100;
                _smm.Publish(new Move(v));
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                var v = Vector3.down * 100;
                _smm.Publish(new Move(v));
            }
        }
        #endregion
    }
}