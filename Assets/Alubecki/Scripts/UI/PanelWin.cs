
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PanelWin : MonoBehaviour {


    [SerializeField] TextMeshProUGUI textStory;
    [SerializeField] Button buttonNextLevel;
    [SerializeField] AudioClip audioClipValidate;


    public bool MustStartNextLevel { get; private set; }


    public void Show(string text) {

        textStory.text = text;
        gameObject.SetActive(true);
        buttonNextLevel.enabled = true;
    }

    public void Hide() {

        MustStartNextLevel = false;

        gameObject.SetActive(false);
    }

    public void OnNextLevelButtonClick() {

        //disable to avoid multiclick
        buttonNextLevel.enabled = false;

        Game.Instance.audioManager.PlaySimpleSound(audioClipValidate);

        MustStartNextLevel = true;
    }

}
