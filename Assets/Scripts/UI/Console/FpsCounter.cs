using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class FpsCounter : MonoBehaviour
    {
        public Text fpsText;
        private string _format = "FPS: {0}";
        private int _frames;
        private Coroutine _updateCounter;

        private IEnumerator UpdateCounter()
        {
            while (true)
            {
                fpsText.text = string.Format(_format, _frames);
                _frames = 0;
                yield return new WaitForSeconds(1f);
            }
        }
        private void Awake()
        {
            _updateCounter = StartCoroutine(UpdateCounter());
        }
        private void Update()
        {
            _frames++;
        }
        private void OnDestroy()
        {
            StopCoroutine(_updateCounter);
        }

    }
}
