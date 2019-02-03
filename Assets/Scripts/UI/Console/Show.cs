using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class Show : MonoBehaviour
    {
        public GameObject rect;
        public Toggle _toggle;

        public void Click()
        {
            rect.SetActive(_toggle.isOn);
        }
    }
}