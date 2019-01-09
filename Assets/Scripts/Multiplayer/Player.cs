using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class Player : MonoBehaviour
    {
        private readonly float _syncPeriod = 0.02f;
        private float _interpTime;
        private float _lastSyncTime;
        private Vector3 _newrbavel;
        private Vector3 _newrbpos;
        private Quaternion _newrbrot;
        private Vector3 _newrbvel;
        private Vector3 _prevrbavel;
        private Vector3 _prevrbpos;
        private Quaternion _prevrbrot;
        private Vector3 _prevrbvel;
        private Rigidbody _rb;
        private float _syncTime;
        private Transform _t;

        public void SynchronizeRigidbody(RigidbodySynchronization message)
        {
            _syncTime = Time.time;
            _interpTime = message.ping / 1000f;
            _prevrbpos = _newrbpos;
            _prevrbvel = _newrbvel;
            _prevrbavel = _newrbavel;
            _prevrbrot = _newrbrot;
            _newrbpos = message.Position;
            _newrbvel = message.Velocity;
            _newrbavel = message.AngularVelocity;
            _newrbrot = message.Rotation;
        }

        #region MonoBehaviour
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _t = GetComponent<Transform>();
            _prevrbpos = _rb.position;
            _prevrbvel = _rb.velocity;
            _prevrbavel = _rb.angularVelocity;
            _prevrbrot = _rb.rotation;
            _prevrbpos = _newrbpos;
            _prevrbvel = _newrbvel;
            _prevrbavel = _newrbavel;
            _prevrbrot = _newrbrot;
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                var v = -Vector3.back * 100;
                MultiplayerManager.Singleton.DeployMessage(new Move(v));
            }

            if (Input.GetKey(KeyCode.A))
            {
                var v = -Vector3.right * 100;
                MultiplayerManager.Singleton.DeployMessage(new Move(v));
            }

            if (Input.GetKey(KeyCode.S))
            {
                var v = Vector3.back * 100;
                MultiplayerManager.Singleton.DeployMessage(new Move(v));
            }

            if (Input.GetKey(KeyCode.D))
            {
                var v = Vector3.right * 100;
                MultiplayerManager.Singleton.DeployMessage(new Move(v));
            }

            if (Input.GetKey(KeyCode.Space))
            {
                var v = Vector3.up * 100;
                MultiplayerManager.Singleton.DeployMessage(new Move(v));
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                var v = Vector3.down * 100;
                MultiplayerManager.Singleton.DeployMessage(new Move(v));
            }

            if (MultiplayerManager.Singleton.Hosting)
                if (Time.time - _lastSyncTime > _syncPeriod)
                {
                    _lastSyncTime = Time.time;
                    MultiplayerManager.Singleton.DeployMessage(new RigidbodySynchronization(_rb));
                }

            if (!MultiplayerManager.Singleton.Hosting)
            {
                var percentage = (Time.time - _syncTime) / (_syncPeriod + _interpTime);
                _t.position = Vector3.Lerp(_prevrbpos, _newrbpos, percentage);
                _t.rotation = Quaternion.Lerp(_prevrbrot, _newrbrot, percentage);
            }
        }
        private void FixedUpdate()
        {
            if (!MultiplayerManager.Singleton.Hosting)
            {
                var percentage = (Time.time - _syncTime) / (_syncPeriod + _interpTime);
                _rb.velocity = Vector3.Lerp(_prevrbvel, _newrbvel, percentage);
                _rb.angularVelocity = Vector3.Lerp(_prevrbavel, _newrbavel, percentage);
            }
        }
        #endregion
    }
}