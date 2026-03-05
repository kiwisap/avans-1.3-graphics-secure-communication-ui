using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Code.Views
{
    public class ConfirmModalView : MonoBehaviour
    {
        public GameObject modalRoot;
        public TMP_Text titleText; // optional
        public TMP_Text bodyText;
        public Button cancelButton;
        public Button confirmButton;

        public void Show() => modalRoot.SetActive(true);
        public void Hide() => modalRoot.SetActive(false);
    }
}
