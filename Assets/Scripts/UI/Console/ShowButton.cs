using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class ShowButton : MonoBehaviour
    {
        public GameObject rect;

        private Toggle _toggle;

        #region MonoBehaviour
        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.isOn = false;
            _toggle.onValueChanged.AddListener(Click);
        }
        #endregion

        private void Click(bool value)
        {
            rect.SetActive(_toggle.isOn);
        }
    }
}