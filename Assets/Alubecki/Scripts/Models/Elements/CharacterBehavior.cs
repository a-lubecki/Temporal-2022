using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class CharacterBehavior : BaseElementBehavior {


    AgeBehavior ageBehavior;
    WeightBehavior weightBehavior;
    StrongnessBehavior strongnessBehavior;
    TemporalAbilityBehavior temporalAbilityBehavior;

    [SerializeField] DataCharacterInChapter dataCharacterInChapter;


    Dictionary<AgeBounds, Height> heightByAgeBounds = new Dictionary<AgeBounds, Height>();
    Dictionary<AgeBounds, Weight> weightByAgeBounds = new Dictionary<AgeBounds, Weight>();
    Dictionary<AgeBounds, Agility> agilityByAgeBounds = new Dictionary<AgeBounds, Agility>();
    Dictionary<AgeBounds, Strongness> strongnessByAgeBounds = new Dictionary<AgeBounds, Strongness>();
    Height height;
    Agility agility;
    LifeStatus lifeStatus;


    public string FullName => dataCharacterInChapter.IsUnknown ? "(unknown)" : dataCharacterInChapter.DataCharacter.FullName;
    public string Description => dataCharacterInChapter.IsUnknown ? "" : "<size=80%>" + dataCharacterInChapter.DataCharacter.Description + "</size>";
    public Team Team => dataCharacterInChapter.Team;
    public SpeciesType SpeciesType => dataCharacterInChapter.DataCharacter.SpeciesType;
    public Gender Gender => dataCharacterInChapter.DataCharacter.Gender;
    public AudioClip AudioClipRotate => dataCharacterInChapter.DataCharacter.AudioClipRotate;
    public AudioClip AudioClipMove => dataCharacterInChapter.DataCharacter.AudioClipMove;
    public AudioClip AudioClipClimb => dataCharacterInChapter.DataCharacter.AudioClipClimb;
    public AudioClip AudioClipFall => dataCharacterInChapter.DataCharacter.AudioClipFall;

    public override bool IsCursorAboveElement => false;
    public override bool CanBeSelected => Team == Team.ALLY;
    public override bool IsPhysicallyWalkableOverBlock => false;
    public override bool CanMove => IsAlive && !IsGrabbing;
    public override bool CanFall => true;
    public bool CanDoAction => IsAlive && !IsGrabbing;
    public bool CanClimb => CanDoAction && (agility == Agility.AGILE || agility == Agility.VERY_AGILE);
    public bool CanJumpHigh => CanDoAction && agility == Agility.VERY_AGILE;

    public MovableObjectBehavior GrabbedMovableObject { get; private set; }
    public bool CanGrab => CanDoAction && GrabbedMovableObject == null;
    public bool IsGrabbing => GrabbedMovableObject != null;
    public bool CanPushOrPull => IsAlive && IsGrabbing;

    public bool IsPlayable => Team == Team.ALLY;
    public bool IsNPC => Team != Team.ALLY;
    public bool IsEnemy => Team == Team.ENEMY;
    public bool IsNeutral => Team == Team.NEUTRAL;

    public bool IsAlive => lifeStatus == LifeStatus.ALIVE;
    public bool IsDead => lifeStatus == LifeStatus.CORPSE || lifeStatus == LifeStatus.DEFINITELY_DEAD;
    public bool IsDefinitelyDead => lifeStatus == LifeStatus.DEFINITELY_DEAD;

    protected override BaseMovement.Factory[] PossibleMovements => new BaseMovement.Factory[] {
        new MovementSimpleMove.Factory(),
        new MovementClimb.Factory(),
        new MovementJumpHigh.Factory(),
        new MovementGrab.Factory(),
        new MovementPushPull.Factory(),
        new MovementRelease.Factory(),
    };

    public override DisplayableCharacteristics DisplayableCharacteristics => new DisplayableCharacteristics(
        GetDisplayableColor(),
        FullName,
        GetTextCharacteristics()
    );

    Color GetDisplayableColor() => Team switch {
        Team.ALLY => new Color(0.3f, 0.8f, 0.3f),
        Team.ENEMY => new Color(0.8f, 0.3f, 0.3f),
        Team.NEUTRAL => new Color(0.8f, 0.6f, 0.3f),
        _ => throw new NotImplementedException()
    };

    string GetTextCharacteristics() {

        var res = string.IsNullOrEmpty(Description) ? "" : Description + "\n\n";

        res += "<b>Species:</b> " + GetTextSpeciesType() + "\n" +
            ageBehavior.DisplayableText + "\n" +
            "<b>Status:</b> " + GetTextStatus() + "\n" +
            weightBehavior.DisplayableText + "\n";

        if (temporalAbilityBehavior != null) {
            res += "\n" + temporalAbilityBehavior.DisplayableText + "\n";
        }

        if (height == Height.SMALL) {
            res += "\n- Can go inside tight locations.";
        }

        if (agility == Agility.AGILE) {
            res += "\n- Can climb.";
        } else if (agility == Agility.VERY_AGILE) {
            res += "\n- Can jump far.";
        }

        if (strongnessBehavior.CanMove(Weight.LIGHT)) {
            res += "\n- Can move <u>Light</u> objects";
        } else if (strongnessBehavior.CanMove(Weight.HEAVY)) {
            res += "\n- Can move <u>Heavy</u> objects";
        } else if (strongnessBehavior.CanMove(Weight.VERY_HEAVY)) {
            res += "\n- Can move <u>Very Heavy</u> objects";
        }

        return res;
    }

    string GetTextTeamPrefix() => Team switch {

        Team.ALLY => "",
        Team.ENEMY => "Enemy ",
        Team.NEUTRAL => "Neutral ",
        _ => throw new NotImplementedException()
    };

    string GetTextSpeciesType() => SpeciesType switch {

        SpeciesType.HUMAN => GetTextTeamPrefix() + "Human",
        SpeciesType.EXTRATERRESTRIAL => GetTextTeamPrefix() + "Extraterrestrial",
        SpeciesType.ROBOT => GetTextTeamPrefix() + "Robot",
        SpeciesType.CREATURE => GetTextTeamPrefix() + "Creature",
        SpeciesType.ENTITY => "Unknown Entity",
        _ => throw new NotImplementedException()
    };

    string GetTextStatus() => lifeStatus switch {

        LifeStatus.UNBORN => "Unborn",
        LifeStatus.ALIVE => "Alive",
        LifeStatus.CORPSE or LifeStatus.DEFINITELY_DEAD => "Dead",
        _ => throw new NotImplementedException()
    };

    void Awake() {

        if (dataCharacterInChapter != null) {
            InitWithCharacterData(dataCharacterInChapter);
        }
    }

    void Update() {

        if (ageBehavior.DidAgeChange) {
            UpdateCharacteristicsWithCurrentAge();
        }
    }

    public override void OnDeselect() {

        ReleaseMovableObject(true);
    }

    public void InitWithCharacterData(DataCharacterInChapter dataCharacterInChapter) {

        this.dataCharacterInChapter = dataCharacterInChapter ?? throw new ArgumentException("Character needs data to be initialized correctly");

        AgeBounds.ConvertAgesToAgeBounds(dataCharacterInChapter.DataCharacter.HeightByAge, heightByAgeBounds);
        AgeBounds.ConvertAgesToAgeBounds(dataCharacterInChapter.DataCharacter.WeightByAge, weightByAgeBounds);
        AgeBounds.ConvertAgesToAgeBounds(dataCharacterInChapter.DataCharacter.AgilityByAge, agilityByAgeBounds);
        AgeBounds.ConvertAgesToAgeBounds(dataCharacterInChapter.DataCharacter.StrongnessByAge, strongnessByAgeBounds);

        ageBehavior = GetComponent<AgeBehavior>() ?? gameObject.AddComponent<AgeBehavior>();
        ageBehavior.InitAges(dataCharacterInChapter.InitialAge, dataCharacterInChapter.DataCharacter.MaxAge);

        weightBehavior = GetComponent<WeightBehavior>() ?? gameObject.AddComponent<WeightBehavior>();
        strongnessBehavior = GetComponent<StrongnessBehavior>() ?? gameObject.AddComponent<StrongnessBehavior>();

        if (dataCharacterInChapter.Ability != null) {
            temporalAbilityBehavior = GetComponent<TemporalAbilityBehavior>() ?? gameObject.AddComponent<TemporalAbilityBehavior>();
            temporalAbilityBehavior.InitData(dataCharacterInChapter.Ability);
        }

        UpdateCharacteristicsWithCurrentAge();
    }

    void UpdateCharacteristicsWithCurrentAge() {

        var age = ageBehavior.CurrentAge;

        height = AgeBounds.FindDataForAge(heightByAgeBounds, age);
        agility = AgeBounds.FindDataForAge(agilityByAgeBounds, age);

        var weight = AgeBounds.FindDataForAge(weightByAgeBounds, age);
        weightBehavior.UpdateWeight(weight);

        var strongness = AgeBounds.FindDataForAge(strongnessByAgeBounds, age);
        strongnessBehavior.UpdateStrongness(strongness);

        if (lifeStatus != LifeStatus.DEFINITELY_DEAD) {
            SetAsAlive();
        }
    }

    public void SetAsAlive() {
        lifeStatus = (ageBehavior.CurrentAge <= 0) ? LifeStatus.UNBORN : ageBehavior.HasReachMaxAge ? LifeStatus.CORPSE : LifeStatus.ALIVE;
    }

    public void SetAsDead() {
        lifeStatus = LifeStatus.DEFINITELY_DEAD;
    }

    public bool CanPushOrPullMovableObject(MovableObjectBehavior movableObject) {
        return IsGrabbingMovableObject(movableObject) &&
            CanPushOrPull &&
            movableObject.CanMove &&
            strongnessBehavior.CanMove(movableObject.GetCumulatedPileWeight());
    }

    public bool IsGrabbingMovableObject(MovableObjectBehavior movableObject) {
        return GrabbedMovableObject == movableObject;
    }

    public void GrabMovableObject(MovableObjectBehavior movableObject, bool animated, Action onComplete = null, float autoRotateDurationSec = 0) {

        if (movableObject == null) {
            throw new ArgumentException();
        }

        if (GrabbedMovableObject == movableObject) {
            //already grabbing
            return;
        }

        if (!CanGrab) {
            throw new InvalidOperationException("Can't grab because already grabbing another MovableObject");
        }

        if (!movableObject.CanBeGrabbed) {
            throw new InvalidOperationException("Can't grab because object is already grabbed");
        }

        GrabbedMovableObject = movableObject;
        GrabbedMovableObject.UpdateGrabbedCharacter(this);

        //always auto rotate before grabbing if not well oriented
        TryLookAt(movableObject.GridPos, autoRotateDurationSec, () => {

            //TODO launch animator grab anim
            onComplete?.Invoke();
        });
    }

    public void ReleaseMovableObject(bool animated, Action onComplete = null) {

        if (GrabbedMovableObject == null) {
            //already released
            return;
        }

        GrabbedMovableObject.UpdateGrabbedCharacter(null);
        GrabbedMovableObject = null;

        //TODO animate tween

        onComplete?.Invoke();
    }

    public override bool TryLookAt(Vector3 nextPos, float durationSec, Action onComplete = null) {

        if (!base.TryLookAt(nextPos, durationSec, onComplete)) {
            return false;
        }

        Game.Instance.audioManager.PlaySimpleSound(AudioClipRotate);

        return true;
    }

    public override bool TryMove(Vector3 nextPos, float durationSec, Action onComplete = null, bool autoRotateBefore = false, float rotateDurationSec = 0) {

        if (!base.TryMove(nextPos, durationSec, onComplete, autoRotateBefore, rotateDurationSec)) {
            return false;
        }

        Game.Instance.audioManager.PlaySimpleSound(AudioClipMove);

        return true;
    }

    public bool TryClimb(Vector3 nextPos, float durationSec, Action onComplete = null, bool autoRotateBefore = false, float rotateDurationSec = 0) {

        if (!base.TryJump(nextPos, durationSec, onComplete, autoRotateBefore, rotateDurationSec)) {
            return false;
        }

        Game.Instance.audioManager.PlaySimpleSound(AudioClipClimb);

        return true;
    }

    public override bool TryJump(Vector3 nextPos, float durationSec, Action onComplete = null, bool autoRotateBefore = false, float rotateDurationSec = 0) {

        if (!base.TryJump(nextPos, durationSec, onComplete, autoRotateBefore, rotateDurationSec)) {
            return false;
        }

        DOTween.Sequence()
            .AppendInterval(durationSec)
            .AppendCallback(() => {
                Game.Instance.audioManager.PlaySimpleSound(AudioClipFall);
            });

        return true;
    }

    public override bool TryFall(int fallHeight, float durationSec, Action onComplete = null) {

        if (!base.TryFall(fallHeight, durationSec, onComplete)) {
            return false;
        }

        //auto release
        ReleaseMovableObject(true);

        DOTween.Sequence()
            .AppendInterval(0.4f * durationSec)
            .AppendCallback(() => {
                Game.Instance.audioManager.PlaySimpleSound(AudioClipFall);
            });

        return true;
    }

    public override bool TryMoveUp(int height, float durationSec, Action onComplete = null) {

        if (!base.TryMoveUp(height, durationSec, onComplete)) {
            return false;
        }

        //auto release
        ReleaseMovableObject(true);

        return true;
    }

    public bool TryPushPull(Vector3 nextPos, float durationSec, Action onComplete = null) {
        return TryMove(nextPos, durationSec, onComplete);
    }

    /// <summary>
    /// Animate character when he can't push or pull because the movable objectis too heavy
    /// </summary>
    public void FailPushPull(Vector3Int gridPos, float durationSec, Action onComplete = null) {

        //TODO trigger character anim

        var s = transform.DOLocalJump(transform.position, 0.1f, 3, durationSec);

        //callback if necessary
        if (onComplete != null) {
            s.AppendInterval(0.01f)
                .OnComplete(() => onComplete());
        }

        Game.Instance.audioManager.PlaySimpleSound(AudioClipMove);
    }

}

public enum Team {

    ALLY,
    ENEMY,
    NEUTRAL
}

public enum SpeciesType {

    HUMAN,
    EXTRATERRESTRIAL,
    ROBOT,
    CREATURE,
    ENTITY
}

public enum Gender {

    UNKNOWN,
    MALE,
    FEMALE
}

public enum Height {

    SMALL,
    TALL
}

public enum Agility {

    NOT_AGILE,
    AGILE,
    VERY_AGILE
}

public enum LifeStatus {

    UNBORN,
    ALIVE,
    CORPSE,
    DEFINITELY_DEAD
}
