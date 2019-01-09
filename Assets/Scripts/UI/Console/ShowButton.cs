using UnityEngine;
using UnityEngine.UI;

namespace UI.Console
{
    public class ShowButton : MonoBehaviour
    {
        private Toggle _toggle;
        public GameObject rect;

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