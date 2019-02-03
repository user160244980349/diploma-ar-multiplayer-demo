using UnityEngine;

namespace UI.LobbyDiscovery
{
    public class Connect : MonoBehaviour
    {
        public Lobby lobby;

        public void Click()
        {
            lobby.Connect();
        }
    }
}
