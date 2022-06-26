using System;
using DG.Tweening;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class BaseViewOverlay : MonoBehaviour {


    CanvasGroup canvasGroup;

    public bool IsVisible => gameObject.activeSelf;

    public bool IsFullyVisible => IsVisible && canvasGroup.alpha >= 1;


    protected virtual void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show() {

        gameObject.SetActive(true);
        FadeView(0, 1, 0);
    }

    public void Hide() {

        gameObject.SetActive(false);
    }

    public void ShowFade() {

        gameObject.SetActive(true);
        FadeView(0, 1, 0.5f);
    }

    public void HideFade() {

        gameObject.SetActive(true);
        FadeView(1, 0, 0.5f, () => Hide());
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
