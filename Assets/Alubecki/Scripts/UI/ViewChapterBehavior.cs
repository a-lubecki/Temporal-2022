using TMPro;
using UnityEngine;


public class ViewChapterBehavior : BaseViewOverlay {


    [SerializeField] TextMeshProUGUI textChapterName;
    [SerializeField] TextMeshProUGUI textChapterStory;


    public void Show(DataChapter dataChapter) {

        textChapterName.text = dataChapter.ChapterName;
        textChapterStory.text = dataChapter.TextChapterStory;

        ShowFade();
    }

    public void OnButtonStartClick() {

        HideFade();
    }

}
