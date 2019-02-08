using Events;
using Multiplayer.Messages.Responses;
using System.Collections;
using UnityEngine;

namespace Multiplayer
{
    public class ObjectView : MonoBehaviour
    {
        public int objectId;

        private Vector3 _newrbavel;
        private Vector3 _newrbpos;
        private Vector3 _newrbvel;
        private Quaternion _newrbrot;

        private Vector3 _prevrbavel;
        private Vector3 _prevrbpos;
        private Vector3 _prevrbvel;
        private Quaternion _prevrbrot;

        private Transform _t;
        private Rigidbody _rb;

        private float _time;
        private float _updatePeiod = 0.13f;
        private Coroutine _sendRBSync;

        private IEnumerator SendRBSync()
        {
            while (true)
            {
                yield return new WaitForSeconds(_updatePeiod);
                if (!_rb.isKinematic && !_rb.IsSleeping())
                {
                    EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new RBSync(objectId, _rb));
                }
            }
        }
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _t = GetComponent<Transform>();
            _sendRBSync = StartCoroutine(SendRBSync());
            
            _newrbpos = _rb.position;
            _newrbvel = _rb.velocity;
            _newrbavel = _rb.angularVelocity;
            _newrbrot = _rb.rotation;
            _prevrbpos = _newrbpos;
            _prevrbvel = _newrbvel;
            _prevrbavel = _newrbavel;
            _prevrbrot = _newrbrot;

            EventManager.Singleton.Publish(GameEventType.RegisterObjectView, this);
        }
        private void Update()
        {
            if (_rb.isKinematic)
            {
                _time += Time.deltaTime;
                var percentage = _time / _updatePeiod;
                _t.position = Vector3.Lerp(_prevrbpos, _newrbpos, percentage);
                _t.rotation = Quaternion.Lerp(_prevrbrot, _newrbrot, percentage);
            }
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Publish(GameEventType.UnregisterObjectView, objectId);
            StopCoroutine(_sendRBSync);
        }

        public void RBSync(RBSync message)
        {
            _time = 0;
            _prevrbpos = _newrbpos;
            _prevrbvel = _newrbvel;
            _prevrbavel = _newrbavel;
            _prevrbrot = _newrbrot;
            _newrbpos = message.Position;
            _newrbvel = message.Velocity;
            _newrbavel = message.AngularVelocity;
            _newrbrot = message.Rotation;
        }
        public void Freeze(bool freeze)
        {
            _rb.isKinematic = freeze;
        }
    }
}