using UnityEngine;
using Events.EventTypes;
using Multiplayer.Messages;
using Events;
using Multiplayer;

public class RBSynchronizator : MonoBehaviour
{
    private SendMultiplayerMessage _smm;

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

    private float _syncPeriod = 0.02f;
    private float _syncTime;
    private float _interpTime;
    private float _lastSyncTime;

    #region MonoBehaviour
    void Start()
    {
        _smm = EventManager.Singleton.GetEvent<SendMultiplayerMessage>();
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
        if (MultiplayerManager.Singleton.Hosting)
        {
            if (Time.time - _lastSyncTime > _syncPeriod)
            {
                _lastSyncTime = Time.time;
                _smm.Publish(new RBSync(_rb));
            }
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

    private void RBSync(RBSync message)
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
}
