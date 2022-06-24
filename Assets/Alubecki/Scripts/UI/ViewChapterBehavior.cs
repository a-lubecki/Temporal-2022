using System;
using DG.Tweening;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class ViewChapterBehavior : MonoBehaviour {


    CanvasGroup canvasGroup;
    [SerializeField] TextMeshProUGUI textChapterName;
    [SerializeField] TextMeshProUGUI textChapterStory;

    public bool IsVisible => gameObject.activeSelf;


    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(DataChapter dataChapter) {

        gameObject.SetActive(true);

        textChapterName.text = dataChapter.ChapterName;
        textChapterStory.text = dataChapter.TextChapterStory;

        FadeView(0, 1, 0);
    }

    public void Hide() {
        
        gameObject.SetActive(false);
    }

    public void OnButtonStartClick() {

        FadeView(1, 0, 0.5f,() => Hide());
    }

    void FadeView(float alphaBegin, float alphaEnd, float durationSec, Action onComplete = null) {

        canvasGroup.alpha = alphaBegin;

        var s = DOTween.To(
            () => canvasGroup.alpha,
            (alpha) => canvasGroup.alpha = alpha,
            alphaEnd,
            durationSec
        );

        //callback if necessary
        if (onComplete != null) {
            s.OnComplete(() => onComplete());
        }
    }

}
