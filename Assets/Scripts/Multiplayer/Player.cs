using Events;
using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class Player : MonoBehaviour
    {
        public int playerId;
        public string playerName;
        public Color playerColor;

        #region MonoBehaviour
        private void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                var v = -Vector3.back * 100;
                EventManager.Singleton.Publish(GameEventType.MultiplayerMessageSend, new Move(playerId, v));
            }

            if (Input.GetKey(KeyCode.A))
            {
                var v = -Vector3.right * 100;
                EventManager.Singleton.Publish(GameEventType.MultiplayerMessageSend, new Move(playerId, v));
            }

            if (Input.GetKey(KeyCode.S))
            {
                var v = Vector3.back * 100;
                EventManager.Singleton.Publish(GameEventType.MultiplayerMessageSend, new Move(playerId, v));
            }

            if (Input.GetKey(KeyCode.D))
            {
                var v = Vector3.right * 100;
                EventManager.Singleton.Publish(GameEventType.MultiplayerMessageSend, new Move(playerId, v));
            }

            if (Input.GetKey(KeyCode.Space))
            {
                var v = Vector3.up * 100;
                EventManager.Singleton.Publish(GameEventType.MultiplayerMessageSend, new Move(playerId, v));
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                var v = Vector3.down * 100;
                EventManager.Singleton.Publish(GameEventType.MultiplayerMessageSend, new Move(playerId, v));
            }
        }
        #endregion
    }
}