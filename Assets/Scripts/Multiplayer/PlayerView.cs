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
            var spawn = GameObject.Find(string.Format("SpawnPoint<{0}>", playerId % 5));
            transform.position = spawn.GetComponent<Transform>().position;
            _rb = GetComponent<Rigidbody>();
        }

        public void Move(Vector3 vector)
        {
            _rb.AddForce(vector);
        }
    }
}