using TMPro;
using UnityEngine;


public class PanelLevelInfo : MonoBehaviour {


    [SerializeField] TextMeshProUGUI textLevelName;


    public void Show(string chapterName, int levelNumber) {

        gameObject.SetActive(true);
        textLevelName.text = chapterName + "\nLevel " + levelNumber;
    }

    public void Hide() {

        gameObject.SetActive(false);
    }

}
