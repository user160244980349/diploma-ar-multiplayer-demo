using Network;
using System.Text;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        private static MultiplayerManager _instance;

        #region MonoBehaviour
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
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
            //if (Client.GetInstance().GetState() == ClientState.Connected)
            //{
            //    NetworkMessage m;
            //    m.type = NetworkMessageType.Service;
            //    m.data = Encoding.ASCII.GetBytes("Boop");
            //    m.length = m.data.Length;
            //    Client.GetInstance().Send(m);
            //}
        }
        #endregion

        public static MultiplayerManager GetInstance()
        {
            return _instance;
        }
    }
}
