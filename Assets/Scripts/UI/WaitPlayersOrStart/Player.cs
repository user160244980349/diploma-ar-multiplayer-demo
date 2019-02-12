using UnityEngine;
using UnityEngine.UI;

namespace UI.WaitPlayersOrStart
{
    public class Player : MonoBehaviour
    {
        public int playerId;
        public string playerName;
        public Text playerIdText;
        public Text playerNameText;

        private void Start()
        {
            playerIdText.text = string.Format("{0}.", playerId);
            if (playerName == "") playerName = "Игрок без ника";
            playerNameText.text = playerName;
        }
    }
}
