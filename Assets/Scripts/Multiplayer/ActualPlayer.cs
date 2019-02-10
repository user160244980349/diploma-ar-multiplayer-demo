using Events;
using Multiplayer.Messages.Requests;
using Multiplayer.Messages.Responses;
using UnityEngine;

public class ActualPlayer : MonoBehaviour
{
    public int Id;
    public string Name;
    public bool Logged;

    private void Start()
    {
        name = "ActualPlayer";
        EventManager.Singleton.Subscribe(GameEventType.LoggingIn, LogIn);
        EventManager.Singleton.Subscribe(GameEventType.LoggingOut, LogOut);
    }
    private void Update()
    {
        if (!Logged) return;

        Vector3 v = Vector3.zero;

        if (Input.GetAxisRaw("Vertical") != 0) v += Input.GetAxisRaw("Vertical") * Vector3.forward * 50;
        if (Input.GetAxisRaw("Horizontal") != 0) v += Input.GetAxisRaw("Horizontal") * Vector3.right * 50;
        if (Input.GetKey(KeyCode.JoystickButton3)) v += Vector3.up * 50;
        if (Input.GetKey(KeyCode.JoystickButton0)) v += Vector3.down * 50;
        if (Input.GetKey(KeyCode.Space)) v += Vector3.up * 50;
        if (Input.GetKey(KeyCode.LeftControl)) v += Vector3.down * 50;

        if (v != Vector3.zero) EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new Move(Id, v));
    }
    private void OnDestroy()
    {
        EventManager.Singleton.Unsubscribe(GameEventType.LoggingIn, LogIn);
        EventManager.Singleton.Unsubscribe(GameEventType.LoggingOut, LogOut);
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
}
