using Network;
using System.Text;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            //if (Client.Instance.GetState() == ClientState.Connected)
            //{
            //    NetworkMessage m;
            //    m.type = NetworkMessageType.Service;
            //    m.data = Encoding.ASCII.GetBytes("Boop");
            //    m.length = m.data.Length;
            //    Client.Instance.Send(m);
            //}
        }
    }
}