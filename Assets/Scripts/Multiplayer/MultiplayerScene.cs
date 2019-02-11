using Events;
using Multiplayer.Messages.Requests;
using Multiplayer.Messages.Responses;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

namespace Multiplayer
{
    class MultiplayerScene : MonoBehaviour, ITrackableEventHandler
    {
        private GameObject _playerPrefab;
        private Dictionary<int, PlayerView> _playerViews;
        private Dictionary<int, ObjectView> _objectViews;
        private TrackableBehaviour _behaviour;
        private Camera _vucamera;
        private bool _freeze;

        private void Start()
        {
            name = "MultiplayerScene";

            _playerPrefab = Resources.Load("Game/Player") as GameObject;
            _playerViews = new Dictionary<int, PlayerView>();
            _objectViews = new Dictionary<int, ObjectView>();

            EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.HostStartedInFallback, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
            EventManager.Singleton.Subscribe(GameEventType.RegisterObjectView, OnRegisterObject);
            EventManager.Singleton.Subscribe(GameEventType.UnregisterObjectView, OnUnregisterObject);
            EventManager.Singleton.Subscribe(GameEventType.StartGame, OnStartGame);
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Unsubscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Unsubscribe(GameEventType.HostStartedInFallback, OnHostStarted);
            EventManager.Singleton.Unsubscribe(GameEventType.ClientStarted, OnClientStarted);
            EventManager.Singleton.Unsubscribe(GameEventType.RegisterObjectView, OnRegisterObject);
            EventManager.Singleton.Unsubscribe(GameEventType.UnregisterObjectView, OnUnregisterObject);
            EventManager.Singleton.Unsubscribe(GameEventType.StartGame, OnStartGame);
        }

        public void OnRegisterObject(object info)
        {
            var objectView = info as ObjectView;
            objectView.objectId = _objectViews.Count + 1;
            _objectViews.Add(objectView.objectId, objectView);
            objectView.Freeze(_freeze);
        }
        public void OnUnregisterObject(object info)
        {
            _objectViews.Remove((int)info);
        }
        public void UpdateRigidbody(TransformSync sync, float duration)
        {
            if (!_objectViews.ContainsKey(sync.ObjectId)) return;
            _objectViews.TryGetValue(sync.ObjectId, out ObjectView objectRepresentation);
            objectRepresentation.TransformSync(sync);
        }
        public void Move(Move move)
        {
            if (!_playerViews.ContainsKey(move.PlayerId)) return;
            _playerViews.TryGetValue(move.PlayerId, out PlayerView playerView);
            playerView.Move(move.Vector);
        }
        public void SpawnPlayer(int id)
        {
            var playerObject = Instantiate(_playerPrefab, transform);
            var playerScript = playerObject.GetComponent<PlayerView>();
            playerScript.playerId = id;
            _playerViews.Add(id, playerScript);
            Debug.LogFormat("MULTIPLAYER_SCENE::Player {0} spawned", id);
        }
        public void DespawnPlayer(int id)
        {
            if (!_playerViews.ContainsKey(id)) return;
            _playerViews.TryGetValue(id, out PlayerView player);
            Destroy(player.gameObject);
            _playerViews.Remove(id);
            Debug.LogFormat("MULTIPLAYER_SCENE::Player {0} despawned", id);
        }
        public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
        {
            int _arMask = 1 << 8;
            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                _vucamera.cullingMask += _arMask;
            }
            else
            {
                _vucamera.cullingMask -= _arMask;
            }
        }

        private void OnStartGame(object info)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                _behaviour = GameObject.Find("VuMark").GetComponent<TrackableBehaviour>();
                _vucamera = GameObject.Find("ARCamera").GetComponent<Camera>();
                _behaviour.RegisterTrackableEventHandler(this);
                Debug.LogFormat("VUCAMERA IS {0}; VUMARK IS {1}", _vucamera, _behaviour);
            }
        }
        private void OnHostStarted(object info)
        {
            _freeze = false;
            FreezeObjects(_freeze);
        }
        private void OnClientStarted(object info)
        {
            _freeze = true;
            FreezeObjects(_freeze);
        }
        private void FreezeObjects(bool freeze)
        {
            foreach (var objectView in _objectViews.Values)
            {
                objectView.Freeze(freeze);
            }
        }
    }
}
