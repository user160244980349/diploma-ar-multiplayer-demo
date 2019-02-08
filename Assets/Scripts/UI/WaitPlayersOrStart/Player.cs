using UnityEngine;
using UnityEngine.UI;

namespace UI.WaitPlayersOrStart
{
    public class Player : MonoBehaviour
    {
        public Text playerNameText;
        public string playerName;

        private void Start()
        {
            playerNameText.text = playerName;
        }
    }
}
