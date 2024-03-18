using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChaosConductor.Shared
{
    class CustomsFileView : MonoBehaviour
    {
        public TextMeshProUGUI customsName;
        public TextMeshProUGUI customsAuthor;
        public TextMeshProUGUI customsVersion;
        private int index;
        private CustomsMenuManager customsMenuManager;

        public void InitCustomsFileView(CustomsMenuManager customsMenuManager, int assignedIndex, string name, string author, string version)
        {
            this.customsMenuManager = customsMenuManager;
            index = assignedIndex;
            customsName.text = name;
            customsAuthor.text = author;
            customsVersion.text = version;

            GetComponent<Button>().onClick.AddListener(OnCustomsButtonClicked);
        }

        private void OnCustomsButtonClicked()
        {
            customsMenuManager.UpdateSelectedCustom(index);
        }
    }
}
