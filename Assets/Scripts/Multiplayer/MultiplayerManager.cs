using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; }

        private void Awake()
        {
            name = "MultiplayerManager";
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
    }
}