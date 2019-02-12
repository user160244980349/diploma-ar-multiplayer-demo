using UnityEngine;
using UnityEngine.UI;

namespace UI.WaitPlayersOrStart
{
    public class Player : MonoBehaviour
    {
        public Text playerIdText;
        public Text playerNameText;

        public int Id {
            get {
                return _id;
            }
            set {
                _id = value;
                playerIdText.text = string.Format("{0}.", _id);
            }
        }
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
                if (_name == "") _name = "Игрок без ника";
                playerNameText.text = _name;
            }
        }

        private int _id;
        private string _name;
    }
}
