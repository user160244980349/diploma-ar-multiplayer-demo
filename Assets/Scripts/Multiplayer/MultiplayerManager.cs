using Network;
using Multiplayer.Messages;
using UnityEngine;
using Network.Messages;
using System;

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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Client.GetInstance().Send(new Boop("Boop from multiplayer layer"));
            }
        }
        #endregion

        public static MultiplayerManager GetInstance()
        {
            return _instance;
        }

        public void DeployMessage(AMultiplayerMessage message)
        {
            Client.GetInstance().Send(message);
        }
        public void PullMessage(AMultiplayerMessage message)
        {
            switch(message.multiplayerMessageType)
            {
                case MultiplayerMessageType.Beep:
                    Debug.Log(string.Format(" > {0}", ((Boop)message).boop));
                    break;

                case MultiplayerMessageType.Move:
                    var player = GameObject.Find("Player");
                    var rb = player.GetComponent<Rigidbody>();
                    rb.AddForce(((Move)message).x, ((Move)message).y, ((Move)message).z);
                    break;

                default:
                    break;
            }
        }
    }
}
