using Events;
using Multiplayer.Messages.Requests;
using Multiplayer.Messages.Responses;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    class MultiplayerScene : MonoBehaviour
    {
        static public MultiplayerScene Singleton;

        private GameObject _playerPrefab;
        private Dictionary<int, PlayerView> _playerViews;
        private Dictionary<int, ObjectView> _objectViews;
        private bool _freeze;

        private void Awake()
        {
            Singleton = this;
        }
        private void Start()
        {
            _playerPrefab = Resources.Load("Game/Player") as GameObject;
            _playerViews = new Dictionary<int, PlayerView>();
            _objectViews = new Dictionary<int, ObjectView>();

            EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.HostStartedInFallback, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
        }

        public void RegisterObject(ObjectView objectRepresentation)
        {
            objectRepresentation.objectId = _objectViews.Count + 1;
            _objectViews.Add(objectRepresentation.objectId, objectRepresentation);
            objectRepresentation.Freeze(_freeze);
        }
        public void UnregisterObject(ObjectView objectRepresentation)
        {
            _objectViews.Remove(objectRepresentation.objectId);
        }
        public void UpdateRigidbody(RBSync sync, float duration)
        {
            if (!_objectViews.ContainsKey(sync.ObjectId)) return;
            _objectViews.TryGetValue(sync.ObjectId, out ObjectView objectRepresentation);
            objectRepresentation.RBSync(sync, duration);
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
