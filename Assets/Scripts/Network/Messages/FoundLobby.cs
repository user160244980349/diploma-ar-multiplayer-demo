using System;

namespace Network.Messages
{
    [Serializable]
    public class FoundLobby : ANetworkMessage
    {
        public string lobbyName;

        public FoundLobby(string name)
        {
            lobbyName = name;
            lowType = NetworkMessageType.FoundLobby;
        }
    }
}