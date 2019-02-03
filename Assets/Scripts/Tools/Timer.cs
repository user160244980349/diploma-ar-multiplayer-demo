using UnityEngine;

namespace Tools
{
    public class Timer : MonoBehaviour
    {
        public float Duration { get; set; }
        public float Remains { get; set; }
        public bool Running { get; set; }
        public bool Elapsed { get; private set; }

        public void Null()
        {
            Remains = 0;
            Elapsed = true;
        }
        public void Discard()
        {
            Remains = Duration;
            Elapsed = false;
        }

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
    }
}

