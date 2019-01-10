using System;
using UnityEngine;

namespace Multiplayer.Messages
{
    [Serializable]
    public class Connect : AMultiplayerMessage
    {
        public string PlayerName { get; private set; }
        public Color PlayerColor {
            get => new Color(_cr, _cg, _cb, _ca);
            private set {
                _cr = value.r;
                _cg = value.g;
                _cb = value.b;
                _ca = value.a;
            }
        }

        private float _ca;
        private float _cb;
        private float _cg;
        private float _cr;

        public Connect(string name, Color color)
        {
            multiplayerMessageType = MultiplayerMessageType.Connect;
            PlayerName = name;
            PlayerColor = color;
        }
    }
}