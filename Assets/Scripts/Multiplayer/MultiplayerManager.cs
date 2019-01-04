using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }
    }
}