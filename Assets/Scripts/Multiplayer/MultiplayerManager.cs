using Events;
using Multiplayer.Messages;
using Network;
using Network.Messages;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; }

        private bool _hosting;
        private int _localPlayerId;
        private Dictionary<int, Player> _players;

        private void Awake()
        {
            name = "MultiplayerManager";
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            _players = new Dictionary<int, Player>();

            EventManager.Singleton.Subscribe(GameEventType.LoggingIn, OnLoggingIn);
            EventManager.Singleton.Subscribe(GameEventType.LoggingOut, OnLoggingOut);
            EventManager.Singleton.Subscribe(GameEventType.SendMultiplayerMessage, OnSendMultiplayerMessage);
            EventManager.Singleton.Subscribe(GameEventType.ReceiveNetworkMessage, OnReceiveNetworkMessage);
            EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
        }
        private void Update()
        {
            
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Unsubscribe(GameEventType.LoggingIn, OnLoggingIn);
            EventManager.Singleton.Unsubscribe(GameEventType.LoggingOut, OnLoggingOut);
            EventManager.Singleton.Unsubscribe(GameEventType.SendMultiplayerMessage, OnSendMultiplayerMessage);
            EventManager.Singleton.Unsubscribe(GameEventType.ReceiveNetworkMessage, OnReceiveNetworkMessage);
            EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
        }

        private void OnHostStarted(object info)
        {
            _hosting = true;
        }
        private void OnClientStarted(object info)
        {
            _hosting = false;
        }
        private void OnLoggingIn(object info)
        {
            EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new LogIn(info as string));
        }
        private void OnLoggingOut(object info)
        {
            EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new LogOut(_localPlayerId));
        }
        private void OnSendMultiplayerMessage(object info)
        {
            EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, info);
        }
        private void OnReceiveNetworkMessage(object info)
        {
            var wrapper = info as MessageWrapper;
            var message = wrapper.message as AMultiplayerMessage;

            switch (message.highType)
            {
                case MultiplayerMessageType.LogIn:
                {
                    var logIn = message as LogIn;
                    if (_hosting)
                    {
                        var except = new ExceptWrapper
                        {
                            message = wrapper.message,
                            connection = wrapper.connection,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkExceptMessage, except);
                        var reply = new ReplyWrapper
                        {
                            message = RegisterPlayer(logIn),
                            connection = wrapper.connection,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkReplyMessage, reply);
                        break;
                    }
                    RegisterPlayer(logIn);
                    break;
                }
                case MultiplayerMessageType.LogOut:
                {
                    var logOut = message as LogOut;
                    if (_hosting)
                    {
                        var except = new ExceptWrapper
                        {
                            message = wrapper.message,
                            connection = wrapper.connection,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkExceptMessage, except);
                        var reply = new ReplyWrapper
                        {
                            message = UnregisterPlayer(logOut),
                            connection = wrapper.connection,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkReplyMessage, reply);
                        break;
                    }
                    UnregisterPlayer(logOut);
                    break;
                }
                case MultiplayerMessageType.LoggedIn:
                {
                    var loggedIn = message as LoggedIn;
                    _localPlayerId = loggedIn.PlayerId;
                    EventManager.Singleton.Publish(GameEventType.LoggedIn, null);
                    break;
                }
                case MultiplayerMessageType.LoggedOut:
                {
                    EventManager.Singleton.Publish(GameEventType.LoggedOut, null);
                    break;
                }
            }
        }
        private LoggedIn RegisterPlayer(LogIn logIn)
        {
            _players.Add(_players.Count, null);
            return new LoggedIn(_players.Count);
        }
        private LoggedOut UnregisterPlayer(LogOut logOut)
        {
            _players.Remove(logOut.PlayerId);
            return new LoggedOut();
        }
    }
}