using System;
using UnityEngine;

namespace Multiplayer
{
    public class PlayerView : MonoBehaviour
    {
        public int playerId;

        private Rigidbody _rb;

        private void Start()
        {
            name = string.Format("PlayerView<{0}>", playerId);
            var spawn = GameObject.Find(string.Format("SpawnPoint<{0}>", playerId % 5));
            var spawnTransform = spawn.GetComponent<Transform>();
            transform.position = spawnTransform.position;
            transform.SetParent(spawnTransform);
            _rb = GetComponent<Rigidbody>();
        }

        public void Move(Vector3 vector)
        {
            _rb.AddForce(vector);
        }
    }
}