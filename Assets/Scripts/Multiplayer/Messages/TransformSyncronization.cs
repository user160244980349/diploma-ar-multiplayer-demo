using System;
using UnityEngine;

namespace Multiplayer.Messages
{
    [Serializable]
    public class TransformSynchronization : AMultiplayerMessage
    {
        public float px;
        public float py;
        public float pz;

        public float qw;
        public float qx;
        public float qy;
        public float qz;

        public float vx;
        public float vy;
        public float vz;

        public TransformSynchronization(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            multiplayerMessageType = MultiplayerMessageType.TransformSynchronization;
            px = position.x;
            py = position.y;
            pz = position.z;
            qw = rotation.w;
            qx = rotation.x;
            qy = rotation.y;
            qz = rotation.z;
            vx = velocity.x;
            vy = velocity.y;
            vz = velocity.z;
        }
        public Vector3 GetPosition()
        {
            return new Vector3(px, py, pz);
        }
        public Quaternion GetRotation()
        {
            return new Quaternion(qx, qy, qz, qw);
        }
        public Vector3 GetVelocity()
        {
            return new Vector3(vx, vy, vz);
        }
    }
}
