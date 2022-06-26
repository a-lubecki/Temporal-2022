using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;


[RequireComponent(typeof(WeightBehavior))]
public class MovableObjectBehavior : BaseElementBehavior {


    AgeBehavior ageBehavior;
    WeightBehavior weightBehavior;
    TemporalAbilityBehavior temporalAbilityBehavior;

    [SerializeField] string objectName;

    [SerializeField] AudioClip audioClipMove;
    [SerializeField] AudioClip audioClipFall;

    public override bool CanMove => (ColliderHeight >= 1);//ex: a destroyed crate with a 0 height can't move
    public override bool CanFall => true;
    public CharacterBehavior GrabbedCharacter { get; private set; }
    public bool CanBeGrabbed => GrabbedCharacter == null;


    public override DisplayableCharacteristics DisplayableCharacteristics => new DisplayableCharacteristics(
        new Color(1, 1, 0.3f),
        objectName,
        "Can be pushed or pulled by characters.\n\n" + GetAgeText() + weightBehavior.DisplayableText + GetTemporalAbilityText()
    );

    string GetAgeText() {
        return (ageBehavior == null) ? "" : ageBehavior.DisplayableText + "\n";
    }

    string GetTemporalAbilityText() {
        return (temporalAbilityBehavior == null) ? "" : "\n\n" + temporalAbilityBehavior.DisplayableText;
    }


    protected virtual void Awake() {

        ageBehavior = GetComponent<AgeBehavior>();
        weightBehavior = GetComponent<WeightBehavior>();
        temporalAbilityBehavior = GetComponent<TemporalAbilityBehavior>();
    }

    public void UpdateGrabbedCharacter(CharacterBehavior character) {
        GrabbedCharacter = character;
    }

    public CumulatedWeight GetCumulatedPileWeight() {

        var pile = Game.Instance.boardBehavior.GetSortedPileOfElements(new Vector2(GridPosX, GridPosZ));

        var y = GridPosY;
        var weights = pile.Where(e => e.GridPosY >= y && e.TryGetComponent<WeightBehavior>(out _))
            .Select(e => e.GetComponent<WeightBehavior>().Weight);

        return new CumulatedWeight(weights);
    }

    public override bool TryMove(Vector3 nextPos, float durationSec, Action onComplete = null, bool autoRotateBefore = false, float rotateDurationSec = 0) {

        if (!base.TryMove(nextPos, durationSec, onComplete, autoRotateBefore, rotateDurationSec)) {
            return false;
        }

        Game.Instance.audioManager.PlaySimpleSound(audioClipMove);

        return true;
    }

    public override bool TryFall(int fallHeight, float durationSec, Action onComplete = null) {

        if (!base.TryFall(fallHeight, durationSec, onComplete)) {
            return false;
        }

        //auto release
        GrabbedCharacter?.ReleaseMovableObject(true);

        //playsound delayed to fit the fall
        DOTween.Sequence()
            .AppendInterval(0.4f * durationSec)
            .AppendCallback(() => {
                Game.Instance.audioManager.PlaySimpleSound(audioClipFall);
            });

        return true;
    }

    public override bool TryMoveUp(int height, float durationSec, Action onComplete = null) {

        if (!base.TryMoveUp(height, durationSec, onComplete)) {
            return false;
        }

        //auto release
        GrabbedCharacter?.ReleaseMovableObject(true);

        return true;
    }

}
