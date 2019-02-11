using Events;
using Multiplayer.Messages.Requests;
using Multiplayer.Messages.Responses;
using System.Collections;
using UnityEngine;

public class ActualPlayer : MonoBehaviour
{
    public int Id;
    public string Name;
    public bool Logged;
    private float _updatePeiod = 0.2f;
    private Coroutine _sendMove;
    private Vector3 _move;

    private IEnumerator SendMove()
    {
        while (true)
        {
            if (_move != Vector3.zero)
            {
                EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new Move(Id, _move));
                _move = Vector3.zero;
            }
            yield return new WaitForSeconds(_updatePeiod);
        }
    }
    private void Start()
    {
        name = "ActualPlayer";

        _move = Vector3.zero;
        _sendMove = StartCoroutine(SendMove());

        EventManager.Singleton.Subscribe(GameEventType.LoggingIn, LogIn);
        EventManager.Singleton.Subscribe(GameEventType.LoggingOut, LogOut);
    }
    private void Update()
    {
        if (!Logged) return;
        if (Input.GetAxisRaw("Vertical") != 0) _move += Input.GetAxisRaw("Vertical") * Vector3.forward * 50;
        if (Input.GetAxisRaw("Horizontal") != 0) _move += Input.GetAxisRaw("Horizontal") * Vector3.right * 50;
        if (Input.GetKey(KeyCode.JoystickButton3)) _move += Vector3.up * 50;
        if (Input.GetKey(KeyCode.JoystickButton0)) _move += Vector3.down * 50;
        if (Input.GetKey(KeyCode.Space)) _move += Vector3.up * 50;
        if (Input.GetKey(KeyCode.LeftControl)) _move += Vector3.down * 50;
    }
    private void OnDestroy()
    {
        StopCoroutine(_sendMove);

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
