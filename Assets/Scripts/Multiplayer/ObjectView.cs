using Events;
using Multiplayer.Messages.Responses;
using System;
using System.Collections;
using UnityEngine;

namespace Multiplayer
{
    public class ObjectView : MonoBehaviour
    {
        public int objectId;

        private Vector3 _newrbpos;
        private Quaternion _newrbrot;

        private Vector3 _prevrbpos;
        private Quaternion _prevrbrot;

        private Vector3 _freezePosition;
        private Quaternion _freezeRotation;

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
                    EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new TransformSync(objectId, _t));
                }
            }
        }
        private void Start()
        {
            if (!name.Contains("Player"))
                name = string.Format("ObjectView<{0}>", objectId);
            _rb = GetComponent<Rigidbody>();
            _t = GetComponent<Transform>();
            _sendRBSync = StartCoroutine(SendRBSync());

            _newrbpos = _t.position;
            _newrbrot = _t.rotation;
            _prevrbpos = _newrbpos;
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

        public void TransformSync(TransformSync message)
        {
            _time = 0;
            _prevrbpos = _newrbpos;
            _prevrbrot = _newrbrot;
            _newrbpos = message.Position;
            _newrbrot = message.Rotation;
        }
        public void Freeze(bool freeze)
        {
            _rb.isKinematic = freeze;
        }
        public void Activate(bool activate)
        {
            gameObject.SetActive(activate);
            if (activate)
            {
                _sendRBSync = StartCoroutine(SendRBSync());
            }
            else
            {
                StopCoroutine(SendRBSync());
            }
        }
    }
}