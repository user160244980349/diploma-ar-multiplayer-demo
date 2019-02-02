using System.Collections.Generic;
using Events;
using Multiplayer.Messages;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; }
        public bool Hosting { get; set; }

        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "MultiplayerManager";
        }
        private void Start()
        {
            //_players = new Dictionary<int, Player>();
            //_objects = new Dictionary<int, RBSynchronizator>();

            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageReceived, PollMM);
            EventManager.Singleton.RegisterListener(GameEventType.MultiplayerMessageSend, SendMM);
        }

        private void SendMM(object info)
        {
            if (Hosting)
            {
                PollMM(info);
            }
            else
            {
                EventManager.Singleton.Publish(GameEventType.NetworkMessageSend, info);
            }
        }
        private void PollMM(object info)
        {
            var message = info as AMultiplayerMessage;
            switch (message.multiplayerMessageType)
            {
                case MultiplayerMessageType.LogIn:
                    LogIn(message as LogIn);
                    break;

                case MultiplayerMessageType.LoggedIn:
                    LoggedIn(message as LoggedIn);
                    break;

                case MultiplayerMessageType.Move:
                    Move(message as Move);
                    break;

                case MultiplayerMessageType.RigidbodySynchronization:
                    SynchronizeRigidbody(message as RBSync);
                    break;

                case MultiplayerMessageType.LogOut:
                    LogOut(message as LogOut);
                    break;
            }
        }

        private void LogIn(LogIn message)
        {
            if (Hosting)
            {
                EventManager.Singleton.Publish(GameEventType.NetworkMessageSend, new LoggedIn(1));
            }
            else
            {
                EventManager.Singleton.Publish(GameEventType.LoggedIn, new LoggedIn(1));
            }
        }
        private void LoggedIn(LoggedIn message)
        {
            EventManager.Singleton.Publish(GameEventType.LoggedIn, new LoggedIn(1));
        }
        private void Move(Move message)
        {

        }
        private void SynchronizeRigidbody(RBSync message)
        {

        }
        private void LogOut(LogOut message)
        {

        }
    }
}