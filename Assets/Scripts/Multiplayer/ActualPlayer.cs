using Events;
using Multiplayer.Messages.Requests;
using Multiplayer.Messages.Responses;
using UnityEngine;

public class ActualPlayer : MonoBehaviour
{
    public int Id;
    public string Name;
    public bool Logged;

    void Start()
    {
        EventManager.Singleton.Subscribe(GameEventType.LoggingIn, LogIn);
        EventManager.Singleton.Subscribe(GameEventType.LoggingOut, LogOut);
    }
    void Update()
    {
        if (!Logged) return;

        Vector3 v = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            v += -Vector3.back * 100;
        }
        if (Input.GetKey(KeyCode.A))
        {
            v += -Vector3.right * 100;
        }
        if (Input.GetKey(KeyCode.S))
        {
            v += Vector3.back * 100;
        }
        if (Input.GetKey(KeyCode.D))
        {
            v += Vector3.right * 100;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            v += Vector3.up * 100;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            v += Vector3.down * 100;
        }
        if (v != Vector3.zero)
        {
            EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new Move(Id, v));
        }
    }

    private void LogIn(object info)
    {
        Name = info as string;
        EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new LogIn(Name));
        Debug.Log("LOCAL_PLAYER::Sending login message");
    }
    private void LogOut(object info)
    {
        EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new LogOut(Id));
        Debug.Log("LOCAL_PLAYER::Sending logout message");
    }
    public void LoggedIn(LoggedIn message)
    {
        Logged = true;
        Id = message.PlayerId;
        EventManager.Singleton.Publish(GameEventType.LoggedIn, null);
        Debug.Log("LOCAL_PLAYER::Got loggedin message");
    }
    public void LoggedOut()
    {
        Logged = false;
        EventManager.Singleton.Publish(GameEventType.LoggedOut, null);
        Debug.Log("LOCAL_PLAYER::Got loggedout message");
    }
}
