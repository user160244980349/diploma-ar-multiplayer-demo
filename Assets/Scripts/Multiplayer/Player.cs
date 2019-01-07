using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class Player : MonoBehaviour
    {
        public int id;

        private static int _count;

        private Rigidbody _rb;

        #region MonoBehaviour
        void Start()
        {
            id = _count++;
            _rb = GetComponent<Rigidbody>();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                var v = -Vector3.back * 1000;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                var v = -Vector3.right * 1000;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                var v = Vector3.back * 1000;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                var v = Vector3.right * 1000;
                _rb.AddForce(v);
                MultiplayerManager.GetInstance().DeployMessage(new Move(v));
            }
        }
        #endregion
    }
}

