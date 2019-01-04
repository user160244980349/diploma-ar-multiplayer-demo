using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class ConsoleManager : MonoBehaviour
    {
        public static ConsoleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public GameObject InstantiateOnScene()
        {
            var canvas = GameObject.Find("Canvas");
            var consolePanel = Instantiate (Resources.Load("UI/Console/Console") as GameObject, canvas.transform);
            return consolePanel;
        }
        
    }
    
}