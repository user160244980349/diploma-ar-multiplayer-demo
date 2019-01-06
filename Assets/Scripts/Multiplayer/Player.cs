using UnityEngine;

namespace Multiplayer
{
    public class Player : MonoBehaviour
    {
        public int id;

        private static int count;

        private Rigidbody rb;

        #region MonoBehaviour
        void Start()
        {
            id = count++;
            rb = GetComponent<Rigidbody>();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                rb.AddForce(-Vector3.back * 100);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                rb.AddForce(-Vector3.right * 100);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                rb.AddForce(Vector3.back * 100);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                rb.AddForce(Vector3.right * 100);
            }
        }
        #endregion
    }
}

