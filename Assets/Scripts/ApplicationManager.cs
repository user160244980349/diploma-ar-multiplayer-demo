using Events.EventTypes;
using Network;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    private static ApplicationManager _instance;
    private ButtonClicked buttonClick;
    private Client client;
    private Host host;

    #region MonoBehaviour
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance == this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LoadScene("Loading");
        DelayedLoadScene("MainMenu", 0.5f);
    }
    private void Update()
    {
    }
    #endregion

    public static ApplicationManager GetInstance()
    {
        return _instance;
    }
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
}
