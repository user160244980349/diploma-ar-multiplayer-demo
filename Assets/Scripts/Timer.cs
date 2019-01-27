using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour
{
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
}
