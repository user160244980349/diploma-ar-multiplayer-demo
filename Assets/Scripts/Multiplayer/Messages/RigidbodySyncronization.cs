using System;
using UnityEngine;

namespace Multiplayer.Messages
{
    [Serializable]
    public class RigidbodySynchronization : AMultiplayerMessage
    {
        public Vector3 Position {
            get => new Vector3(_px, _py, _pz);
            private set {
                _px = value.x;
                _py = value.y;
                _pz = value.z;
            }
        }
        public Vector3 Velocity {
            get => new Vector3(_vx, _vy, _vz);
            private set {
                _vx = value.x;
                _vy = value.y;
                _vz = value.z;
            }
        }
        public Vector3 AngularVelocity {
            get => new Vector3(_avx, _avy, _avz);
            private set {
                _avx = value.x;
                _avy = value.y;
                _avz = value.z;
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

        public RigidbodySynchronization(Rigidbody rb)
        {
            multiplayerMessageType = MultiplayerMessageType.RigidbodySynchronization;
            Position = rb.position;
            Velocity = rb.velocity;
            AngularVelocity = rb.angularVelocity;
            Rotation = rb.rotation;
        }
    }
}