using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class Player : MonoBehaviour
    {
        private Rigidbody _rb;
        private float _syncPeriod = 0.2f;
        private float _lastSyncTime;

        private Vector3 _prevPosition;
        private Quaternion _prevRotation;
        private Vector3 _prevVelocity;

        float syncTime;
        float syncDelay = 5;
        Vector3 newrbpos;
        Quaternion newrbrot;
        Vector3 newrbvel;

        #region MonoBehaviour
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                var v = -Vector3.back * 100;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
            if (Input.GetKey(KeyCode.A))
            {
                var v = -Vector3.right * 100;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
            if (Input.GetKey(KeyCode.S))
            {
                var v = Vector3.back * 100;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
            if (Input.GetKey(KeyCode.D))
            {
                var v = Vector3.right * 100;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
            if (Input.GetKey(KeyCode.Space))
            {
                var v = Vector3.up * 100;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }

            if (Time.time - _lastSyncTime > _syncPeriod)
            {
                MultiplayerManager.GetInstance().DeployMessage(new TransformSynchronization(_rb.position, _rb.rotation, _rb.velocity));
                _lastSyncTime = Time.time;
            }
        }
        private void FixedUpdate()
        {
            var percentage = (Time.time - syncTime) / syncDelay;
            _rb.position = Vector3.Lerp(_rb.position, newrbpos, percentage);
            _rb.rotation = Quaternion.Lerp(_rb.rotation, newrbrot, percentage);
            _rb.velocity = Vector3.Lerp(_rb.velocity, newrbvel, percentage);
        }
        #endregion

        public void UpdateTransform(TransformSynchronization message)
        {
            if (Time.time - _lastSyncTime > _syncPeriod)
            {
                syncTime = Time.time;
                newrbpos = message.GetPosition();
                newrbrot = message.GetRotation();
                newrbvel = message.GetVelocity();
            }
        }
    }
}

