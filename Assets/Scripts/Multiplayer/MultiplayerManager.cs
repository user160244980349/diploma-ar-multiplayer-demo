using UnityEngine;

namespace Multiplayer {

    public class MultiplayerManager : MonoBehaviour {

        static MultiplayerManager instance = null;

        private void Awake () {

            if (instance == null) {
                instance = this;
            } else {
                Destroy(this);
            }

        }

        public static MultiplayerManager GetInstance () {
            return instance;
        }

    }

}
