using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Code.Views
{
    public class CreateEnvironmentModalView : MonoBehaviour
    {
        public GameObject modalRoot;

        public TMP_InputField nameInput;
        public TMP_InputField widthInput;
        public TMP_InputField heightInput;

        public Button cancelButton;
        public Button createButton;

        public void Show()
        {
            modalRoot.SetActive(true);

            if (string.IsNullOrWhiteSpace(widthInput.text)) widthInput.SetTextWithoutNotify("20");
            if (string.IsNullOrWhiteSpace(heightInput.text)) heightInput.SetTextWithoutNotify("10");
        }

        public void Hide() => modalRoot.SetActive(false);
    }
}
