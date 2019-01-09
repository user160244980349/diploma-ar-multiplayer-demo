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
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public void DelayedLoadScene(string sceneName, float time)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, time));
    }
    private IEnumerator LoadSceneCoroutine(string sceneName, float time)
    {
        yield return new WaitForSeconds(time);
        LoadScene(sceneName);
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