using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class ShowButton : MonoBehaviour
    {
        public GameObject rect;

        private Toggle _toggle;

        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.isOn = false;
            _toggle.onValueChanged.AddListener(Click);
        }

        private void Click(bool value)
        {
            rect.SetActive(_toggle.isOn);
        }
    }
}