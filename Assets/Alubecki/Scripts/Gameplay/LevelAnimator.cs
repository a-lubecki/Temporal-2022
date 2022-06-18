using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;


public class LevelAnimator : MonoBehaviour {


    public const float DELAY_BEFORE_JUMP_SEC = 0.1f;
    public const float DURATION_ANIM_JUMP_SEC = 0.6f;
    public const float DURATION_ANIM_ROTATE_SEC = 0.7f;
    public const float DELAY_BEFORE_ROTATE_SEC = 0.1f;
    public const float DURATION_ANIM_SCALE_SEC = 0.6f;
    public const float DURATION_TOTAL_SEC = 1;//must be greater than the longest anims to avoid bugs


    public void AnimateLevelShow(Transform trBoard, Transform trCurrentLevel, Action onComplete) {

        AnimateLevel(trBoard, trCurrentLevel, true);

        StartCoroutine(CallOnCompleteAfterDelay(onComplete));
    }

    public void AnimateLevelHide(Transform trBoard, Transform trCurrentLevel, Action onComplete) {

        AnimateLevel(trBoard, trCurrentLevel, false);

        StartCoroutine(CallOnCompleteAfterDelay(onComplete));
    }

    void AnimateLevel(Transform trBoard, Transform trCurrentLevel, bool mustShow) {

        //make the board jump after delay
        DOTween.Sequence()
            .PrependInterval(DELAY_BEFORE_JUMP_SEC)
            .Append(trBoard.DOLocalJump(Vector3.zero, 2, 1, DURATION_ANIM_JUMP_SEC, false));

        //rotate the level after delay
        var angleStart = mustShow ? 180 : 0;
        var angleEnd = mustShow ? 0 : -180;
        trCurrentLevel.localRotation = Quaternion.Euler(new Vector3(0, 0, angleStart));
        DOTween.Sequence()
            .PrependInterval(DELAY_BEFORE_ROTATE_SEC)
            .Append(trCurrentLevel.DOLocalRotate(new Vector3(0, 0, angleEnd), DURATION_ANIM_ROTATE_SEC).SetEase(Ease.OutBack));

        //scale the level
        var scaleStart = mustShow ? 0 : 1;
        var scaleEnd = mustShow ? 1 : 0;
        trCurrentLevel.localScale = Vector3.one * scaleStart;
        trCurrentLevel.DOScale(Vector3.one * scaleEnd, DURATION_ANIM_SCALE_SEC).SetEase(Ease.OutBack);
    }

    IEnumerator CallOnCompleteAfterDelay(Action onComplete) {

        yield return new WaitForSeconds(DURATION_TOTAL_SEC);

        onComplete();
    }

}
