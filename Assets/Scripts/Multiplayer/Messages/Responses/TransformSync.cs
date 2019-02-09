using System;
using UnityEngine;

namespace Multiplayer.Messages.Responses
{
    [Serializable]
    public class TransformSync : AMultiplayerMessage
    {
        public int ObjectId { get; private set; }
        public Vector3 Position {
            get => new Vector3(_px, _py, _pz);
            private set {
                _px = value.x;
                _py = value.y;
                _pz = value.z;
            }
        }
        public Quaternion Rotation {
            get => new Quaternion(_qx, _qy, _qz, _qw);
            private set {
                _qx = value.x;
                _qy = value.y;
                _qz = value.z;
                _qw = value.w;
            }
        }

        private float _avx;
        private float _avy;
        private float _avz;
        private float _px;
        private float _py;
        private float _pz;
        private float _qw;
        private float _qx;
        private float _qy;
        private float _qz;
        private float _vx;
        private float _vy;
        private float _vz;

        public TransformSync(int objectId, Transform t)
        {
            highType = MultiplayerMessageType.RBSync;
            ObjectId = objectId;
            Position = t.position;
            Rotation = t.rotation;
        }
    }
}