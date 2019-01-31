using System;
using UnityEngine;

namespace Multiplayer.Messages
{
    [Serializable]
    public class Move : AMultiplayerMessage
    {
        public int PlayerId { get; private set; }
        public Vector3 Vector {
            get => new Vector3(_x, _y, _z);
            private set {
                _x = value.x;
                _y = value.y;
                _z = value.z;
            }
        }

        private float _x;
        private float _y;
        private float _z;

        public Move(int playerId, Vector3 v)
        {
            multiplayerMessageType = MultiplayerMessageType.Move;
            PlayerId = playerId;
            Vector = v;
        }
    }
}