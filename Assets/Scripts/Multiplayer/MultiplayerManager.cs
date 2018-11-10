using UnityEngine;

namespace Diploma.Multiplayer {

    public class MultiplayerManager : MonoBehaviour {

        static MultiplayerManager instance = null;

        void Awake () {

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
