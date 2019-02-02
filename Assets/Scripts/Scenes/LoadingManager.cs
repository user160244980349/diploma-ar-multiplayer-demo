using UI.Console;
using UnityEngine;

namespace Scenes
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
        }
        private void Start()
        {
            ConsoleManager.Singleton.InstantiateConsole();
        }
    }
}