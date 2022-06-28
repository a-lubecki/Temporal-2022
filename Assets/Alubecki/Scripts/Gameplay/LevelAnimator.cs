using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;


public class LevelAnimator : MonoBehaviour {


    public const float DURATION_TOTAL_SEC = 1;//must be greater than the longest anims to avoid bugs


    [SerializeField] AudioClip audioClipLevelShow;
    [SerializeField] AudioClip audioClipLevelHide;


    public void AnimateLevelShow(Transform trBoard, Transform trCurrentLevel, Action onComplete) {

        Game.Instance.audioManager.PlaySimpleSound(audioClipLevelShow);

        //make the board jump after delay
        DOTween.Sequence()
            .PrependInterval(0.1f)
            .Append(trBoard.DOLocalJump(Vector3.zero, 2, 1, 0.6f, false));

        //rotate the level after delay
        trCurrentLevel.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
        DOTween.Sequence()
            .PrependInterval(0.1f)
            .Append(trCurrentLevel.DOLocalRotate(new Vector3(0, 0, 0), 0.7f).SetEase(Ease.OutBack));

        //scale the level
        trCurrentLevel.localScale = Vector3.zero;
        trCurrentLevel.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);

        StartCoroutine(CallOnCompleteAfterDelay(onComplete));
    }

    public void AnimateLevelHide(Transform trBoard, Transform trCurrentLevel, Action onComplete) {

        Game.Instance.audioManager.PlaySimpleSound(audioClipLevelHide);

        //make the board jump after delay
        trBoard.DOLocalJump(Vector3.zero, 3, 1, 0.6f, false);

        //scale the level
        trCurrentLevel.localScale = Vector3.one;
        trCurrentLevel.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InBack);

        StartCoroutine(CallOnCompleteAfterDelay(onComplete));
    }

    IEnumerator CallOnCompleteAfterDelay(Action onComplete) {

        yield return new WaitForSeconds(DURATION_TOTAL_SEC);

        onComplete();
    }

}
