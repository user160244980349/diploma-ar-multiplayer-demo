using System.Collections;
using Events.EventTypes;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    private ButtonClicked _buttonClick;
    private Client _client;
    private Host _host;
    public static ApplicationManager Singleton { get; private set; }
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
    public void DelayedLoadScene(string name, float time)
    {
        StartCoroutine(LoadSceneCoroutine(name, time));
    }
    private IEnumerator LoadSceneCoroutine(string name, float time)
    {
        yield return new WaitForSeconds(time);
        LoadScene(name);
    }

    #region MonoBehaviour
    private void Awake()
    {
        if (Singleton == null)
            Singleton = this;
        else if (Singleton == this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LoadScene("Loading");
        DelayedLoadScene("MainMenu", 0.25f);
    }
    #endregion
}