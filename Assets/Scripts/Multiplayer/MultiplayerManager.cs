using Events;
using Multiplayer.Messages;
using Multiplayer.Messages.Requests;
using Multiplayer.Messages.Responses;
using Network.Messages.Wrappers;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; }

        private GameObject _scenePrefab;
        private GameObject _playerPrefab;

        private bool _hosting;
        private bool _gameStarted;
        private ActualPlayer _actualPlayer;
        private MultiplayerScene _multiplayerScene;
        private Dictionary<int, PlayerModel> _playerModels;

        private void Awake()
        {
            name = "MultiplayerManager";
            if (Singleton == null)
                Singleton = this;
            else
            {
                Destroy(Singleton.gameObject);
                Singleton = this;
            }
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            Debug.Log("MULTIPLAYER_MANAGER::Instantiating");

            _scenePrefab = Resources.Load("Multiplayer/MultiplayerScene") as GameObject;
            var sceneObject = Instantiate(_scenePrefab, transform) as GameObject;
            _multiplayerScene = sceneObject.GetComponent<MultiplayerScene>();

            _playerPrefab = Resources.Load("Multiplayer/ActualPlayer") as GameObject;
            var playerObject = Instantiate(_playerPrefab, transform) as GameObject;
            _actualPlayer = playerObject.GetComponent<ActualPlayer>();

            _playerModels = new Dictionary<int, PlayerModel>();

            EventManager.Singleton.Subscribe(GameEventType.SendMultiplayerMessage, OnSendMultiplayerMessage);
            EventManager.Singleton.Subscribe(GameEventType.ReceiveNetworkMessage, OnReceiveNetworkMessage);
            EventManager.Singleton.Subscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.HostStartedInFallback, OnHostStarted);
            EventManager.Singleton.Subscribe(GameEventType.ClientStarted, OnClientStarted);
            EventManager.Singleton.Subscribe(GameEventType.StartGame, OnStartGame);
            EventManager.Singleton.Subscribe(GameEventType.PublishPlayersList, OnPublishPlayers);
        }
        private void OnDestroy()
        {
            Debug.Log("MULTIPLAYER_MANAGER::Destroying");

            EventManager.Singleton.Unsubscribe(GameEventType.SendMultiplayerMessage, OnSendMultiplayerMessage);
            EventManager.Singleton.Unsubscribe(GameEventType.ReceiveNetworkMessage, OnReceiveNetworkMessage);
            EventManager.Singleton.Unsubscribe(GameEventType.HostStarted, OnHostStarted);
            EventManager.Singleton.Unsubscribe(GameEventType.HostStartedInFallback, OnHostStarted);
            EventManager.Singleton.Unsubscribe(GameEventType.ClientStarted, OnClientStarted);
            EventManager.Singleton.Unsubscribe(GameEventType.StartGame, OnStartGame);
            EventManager.Singleton.Unsubscribe(GameEventType.PublishPlayersList, OnPublishPlayers);

            Destroy(_actualPlayer);
            Destroy(_multiplayerScene);
        }

        private void OnHostStarted(object info)
        {
            _hosting = true;
        }
        private void OnClientStarted(object info)
        {
            _hosting = false;
        }

        private void OnSendMultiplayerMessage(object info)
        {
            var message = info as AMultiplayerMessage;

            if (_hosting)
            {
                /*
                 * Поведение хоста при отправке сообщения
                 */
                switch (message.highType)
                {
                    // само-логин
                    case MultiplayerMessageType.LogIn:
                    {
                        _actualPlayer.LoggedIn(RegisterPlayer(message as LogIn));
                        var send = new SendWrapper
                        {
                            message = message,
                            channel = 0,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, send);
                        break;
                    }
                    case MultiplayerMessageType.SessionStarted:
                    {
                        EventManager.Singleton.Publish(GameEventType.SessionStarted, null);
                        var send = new SendWrapper
                        {
                            message = message,
                            channel = 0,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, send);
                        break;
                    }
                    // само-движение
                    case MultiplayerMessageType.Move:
                    {
                        _multiplayerScene.Move(message as Move);
                        break;
                    }
                    case MultiplayerMessageType.RBSync:
                    {
                        var send = new SendWrapper
                        {
                            message = message,
                            channel = 2,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, send);
                        break;
                    }
                    case MultiplayerMessageType.LogOut:
                    {
                        var send = new SendWrapper
                        {
                            message = new SessionEnded(),
                            channel = 0,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, send);
                        EventManager.Singleton.Publish(GameEventType.LoggedOut, null);
                        break;
                    }
                    default:
                    {

                        break;
                    }
                }
                return;
            }

            /*
             * Поведение клиента при отправке сообщения
             */
            switch (message.highType)
            {
                case MultiplayerMessageType.RBSync:
                {
                    
                    break;
                }
                default:
                {
                    var send = new SendWrapper
                    {
                        message = message,
                        channel = 1,
                    };
                    EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, send);
                    break;
                }
            }
        }
        private void OnReceiveNetworkMessage(object info)
        {
            var wrapper = (ReceiveWrapper)info;
            var message = wrapper.message as AMultiplayerMessage;

            if (_hosting)
            {
                /*
                 * Поведение хоста при приеме сообщения
                 */
                switch (message.highType)
                {
                    case MultiplayerMessageType.LogIn:
                    {
                        var send = new SendWrapper
                        {
                            message = wrapper.message,
                            channel = 0,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, send);

                        foreach (var value in _playerModels.Values)
                        {
                            var player = new ReplyWrapper
                            {
                                message = new LogIn(value.playerName),
                                connection = wrapper.connection,
                                channel = 0,
                            };
                            EventManager.Singleton.Publish(GameEventType.SendNetworkReplyMessage, player);
                        }

                        var reply = new ReplyWrapper
                        {
                            message = RegisterPlayer(message as LogIn),
                            connection = wrapper.connection,
                            channel = 0,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkReplyMessage, reply);
                        break;
                    }
                    case MultiplayerMessageType.Move:
                    {
                        _multiplayerScene.Move(message as Move);
                        break;
                    }
                    case MultiplayerMessageType.LogOut:
                    {
                        var send = new SendWrapper
                        {
                            message = wrapper.message,
                            channel = 0,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkMessage, send);

                        var reply = new ReplyWrapper
                        {
                            message = UnregisterPlayer(message as LogOut),
                            connection = wrapper.connection,
                            channel = 0,
                        };
                        EventManager.Singleton.Publish(GameEventType.SendNetworkReplyMessage, reply);
                        break;
                    }
                }
                return;
            }

            /*
             * Поведение клиента при приеме сообщения
             */
            switch (message.highType)
            {
                case MultiplayerMessageType.LogIn:
                {
                    RegisterPlayer(message as LogIn);
                    break;
                }
                case MultiplayerMessageType.LoggedIn:
                {
                    _actualPlayer.LoggedIn(message as LoggedIn);
                    break;
                }
                case MultiplayerMessageType.SessionStarted:
                {
                    EventManager.Singleton.Publish(GameEventType.SessionStarted, null);
                    break;
                }
                case MultiplayerMessageType.RBSync:
                {
                    _multiplayerScene.UpdateRigidbody(message as RBSync, wrapper.ping);
                    break;
                }
                case MultiplayerMessageType.LogOut:
                {
                    UnregisterPlayer(message as LogOut);
                    break;
                }
                case MultiplayerMessageType.SessionEnded:
                {
                    _actualPlayer.LoggedOut();
                    break;
                }
                case MultiplayerMessageType.LoggedOut:
                {
                    _actualPlayer.LoggedOut();
                    break;
                }
            }
        }

        private void OnPublishPlayers(object info)
        {
            foreach (var player in _playerModels.Values)
            {
                EventManager.Singleton.Publish(GameEventType.PlayerRegistered, player);
            }
        }
        private void OnStartGame(object info)
        {
            _gameStarted = true;
            foreach (var player in _playerModels.Keys)
            {
                _multiplayerScene.SpawnPlayer(player);
            }
            Debug.Log("MULTIPLAYER_MANAGER::Game started");
        }
        private LoggedIn RegisterPlayer(LogIn logIn)
        {
            var id = _playerModels.Count + 1;
            var player = new PlayerModel
            {
                playerId = id,
                playerName = logIn.PlayerName,
            };
            _playerModels.Add(id, player);

            if (_gameStarted)
                _multiplayerScene.SpawnPlayer(id);

            EventManager.Singleton.Publish(GameEventType.PlayerRegistered, player);

            Debug.LogFormat("MULTIPLAYER_MANAGER::Player {0} registered as {1}", id, logIn.PlayerName);
            return new LoggedIn(id);
        }
        private LoggedOut UnregisterPlayer(LogOut logOut)
        {
            _playerModels.Remove(logOut.PlayerId);

            if (_gameStarted)
                _multiplayerScene.DespawnPlayer(logOut.PlayerId);

            EventManager.Singleton.Publish(GameEventType.PlayerUnregistered, logOut.PlayerId);

            Debug.LogFormat("MULTIPLAYER_MANAGER::Player {0} unregistered", logOut.PlayerId);
            return new LoggedOut();
        }
    }
}