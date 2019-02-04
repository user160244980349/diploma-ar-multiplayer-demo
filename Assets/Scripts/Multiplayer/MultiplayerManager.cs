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
        private bool _loggedIn;
        private int _localPlayerId;
        private Dictionary<int, string> _players;

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
            _players = new Dictionary<int, string>();

            EventManager.Singleton.Subscribe(GameEventType.LoggingIn, OnLoggingIn);
            EventManager.Singleton.Subscribe(GameEventType.LoggingOut, OnLoggingOut);
            EventManager.Singleton.Subscribe(GameEventType.SendMultiplayerMessage, OnSendMultiplayerMessage);
            EventManager.Singleton.Subscribe(GameEventType.ReceiveNetworkMessage, OnReceiveNetworkMessage);
            EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
        }
        private void Update()
        {
            if (_loggedIn)
                EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, new Boop());
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
            if (_hosting)
            {
                var message = info as AMultiplayerMessage;
                if (message.highType == MultiplayerMessageType.LogIn)
                {
                    _loggedIn = true;
                    RegisterPlayer(message as LogIn);
                    EventManager.Singleton.Publish(GameEventType.LoggedIn, info);
                }
                if (message.highType == MultiplayerMessageType.LogOut)
                {
                    _loggedIn = false;
                    UnregisterPlayer(message as LogOut);
                    EventManager.Singleton.Publish(GameEventType.LoggedOut, null);
                }
            }
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
                        foreach (var value in _players.Values)
                        {
                            var player = new ReplyWrapper
                            {
                                message = new LogIn(value),
                                connection = wrapper.connection,
                            };
                            EventManager.Singleton.Publish(GameEventType.SendNetworkReplyMessage, player);
                        }
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
                    _loggedIn = true;
                    var loggedIn = message as LoggedIn;
                    _localPlayerId = loggedIn.PlayerId;
                    EventManager.Singleton.Publish(GameEventType.LoggedIn, null);
                    break;
                }
                case MultiplayerMessageType.LoggedOut:
                {
                    _loggedIn = false;
                    EventManager.Singleton.Publish(GameEventType.LoggedOut, null);
                    break;
                }
                case MultiplayerMessageType.Boop:
                {
                    Debug.Log("BOOB");
                    break;
                }
            }
        }
        private LoggedIn RegisterPlayer(LogIn logIn)
        {
            _players.Add(_players.Count, logIn.PlayerName);
            Debug.LogFormat("MULTIPLAYER::Player {0} logged in as {1}", _players.Count, logIn.PlayerName);
            return new LoggedIn(_players.Count);
        }
        private LoggedOut UnregisterPlayer(LogOut logOut)
        {
            Debug.LogFormat("MULTIPLAYER::Player {0} logged out", logOut.PlayerId);
            _players.Remove(logOut.PlayerId);
            return new LoggedOut();
        }
    }
}