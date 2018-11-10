using UnityEngine;

namespace Diploma.Scenes {

    public class SceneManager : MonoBehaviour {

        static SceneManager instance = null;

        void Awake () {

            if (instance == null) {
                instance = this;
            } else {
                Destroy(this);
            }

        }

        public static SceneManager GetInstance () {
            return instance;
        }

        public void LoadScene (string name) {
            UnityEngine
                .SceneManagement
                .SceneManager
                .LoadScene(name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

    }

}
