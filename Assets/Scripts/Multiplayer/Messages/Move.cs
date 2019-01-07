using System;
using UnityEngine;

namespace Multiplayer.Messages
{
    [Serializable]
    public class Move : AMultiplayerMessage
    {
        public float x;
        public float y;
        public float z;

        public Move(Vector3 v)
        {
            multiplayerMessageType = MultiplayerMessageType.Move;
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }
}
