using Events;
using Events.EventTypes;
using Multiplayer;
using Network;
using UI.Console;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    private ButtonClicked buttonClick;
    private Client client;
    private Host host;

    public static ApplicationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // subsystems
            gameObject.AddComponent<EventManager>();
            gameObject.AddComponent<NetworkManager>();
            gameObject.AddComponent<Host>();
            gameObject.AddComponent<Client>();
            gameObject.AddComponent<MultiplayerManager>();
            gameObject.AddComponent<ConsoleManager>();
        }
        else if (Instance == this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadScene("MainMenu");
    }

    public void LoadScene(string name)
    {
        SceneManager
            .LoadScene(name, LoadSceneMode.Single);
    }
}