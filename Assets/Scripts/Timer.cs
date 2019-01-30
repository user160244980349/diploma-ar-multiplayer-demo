using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour
{
    public float Duration { get; set; }
    public float Remains { get; set; }
    public bool Running { get; set; }
    public bool Elapsed { get; private set; }

    #region MonoBehaviour
    private void Update()
    {
        if (Running)
        {
            Remains -= Time.deltaTime;

            if (Remains < 0)
                Elapsed = true;
            else
                Elapsed = false;
        }
    }
    #endregion

    public void Null()
    {
        Remains = 0;
    }
    public void Discard()
    {
        Remains = Duration;
    }
}
