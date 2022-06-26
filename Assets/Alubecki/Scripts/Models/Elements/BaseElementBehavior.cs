using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class BaseElementBehavior : GridPosBehavior, IMementoOriginator {


    [SerializeField] bool preventFromWalkingOn;

    /// <summary>
    /// When a ground is invisible, only NPC can go on it to enter / leave the board
    /// </summary>
    public bool IsInvisible => HasInvisibleMeshes || !gameObject.activeInHierarchy;
    public bool HasInvisibleMeshes => GetComponentsInChildren<MeshRenderer>().Where(mr => mr.enabled).Count() <= 0;
    public virtual bool IsCursorAboveElement => true;
    public virtual bool CanBeSelected => false;
    public int ColliderHeight => GetComponentInChildren<OverridenHeightBehavior>()?.ColliderHeight ?? 1;
    public virtual bool IsPhysicallyWalkableInBlock => !IsInvisible && !preventFromWalkingOn && ColliderHeight <= 0;
    public virtual bool IsPhysicallyWalkableOverBlock => !IsInvisible && !preventFromWalkingOn && ColliderHeight > 0;
    public virtual bool IsTheoricallyWalkableOverBlock => !preventFromWalkingOn && ColliderHeight > 0;
    public virtual bool CanMove => false;
    public virtual bool CanMoveOverInvisibleBlocks => false;
    public virtual bool CanFall => false;
    protected virtual BaseMovement.Factory[] PossibleMovements => null;

    public bool HasDisplayableCharacteristics => DisplayableCharacteristics != null;
    public abstract DisplayableCharacteristics DisplayableCharacteristics { get; }


    public void SetMeshesVisible(bool isVisible) {

        if (TryGetComponent<AgeBehavior>(out var ageBehavior)) {

            //let the age behavior udpdate meshes
            ageBehavior.SetMeshesEnabled(isVisible);

        } else {

            //update manually
            var meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in meshRenderers) {
                mr.enabled = isVisible;
            }
        }
    }

    public int GetColliderHeightForAge(int age) {
        return GetComponent<AgeBehavior>()?.FindMeshForAge(age)?.GetComponent<OverridenHeightBehavior>()?.ColliderHeight ?? 1;
    }

    public IEnumerable<BaseMovement.Factory> GetPossibleMovements() {

        if (PossibleMovements == null) {
            return null;
        }

        //defensive copy
        return new List<BaseMovement.Factory>(PossibleMovements);
    }

    public virtual void OnDeselect() {
        //override if necessary
    }

    public IMementoSnapshot NewSnapshot() {

        var ageBehavior = GetComponent<AgeBehavior>();
        var ageParadoxBehavior = ageBehavior as AgeParadoxBehavior;

        return new MementoSnapshotElement(
            GetInstanceID(),
            transform.localPosition,
            transform.localRotation,
            (this as CharacterBehavior)?.IsDead ?? false,
            ageBehavior?.CurrentAge ?? 0,
            ageParadoxBehavior?.IsInParadoxState ?? false,
            ageParadoxBehavior?.ParadoxAge ?? 0
        );
    }

    public void Restore(IMementoSnapshot snapshot) {

        if (snapshot is not MementoSnapshotElement) {
            throw new ArgumentException("Wrong snapshot type, waiting for a MementoSnapshotElement");
        }

        var s = (MementoSnapshotElement)snapshot;

        transform.localPosition = s.localPos;
        transform.localRotation = s.localRot;

        var c = this as CharacterBehavior;
        if (c != null) {
            if (s.isDead) {
                c.SetAsDead();
            } else {
                c.SetAsAlive();
            }
        }

        GetComponent<AgeBehavior>()?.InitCurrentAge(s.age);
    }

    /// <summary>
    /// Animate all differences between this elem and the snapshot.
    /// Return if is animating or not.
    /// </summary>
    public bool AnimateChanges(MementoSnapshotElement snapshot, float durationSec, Action onComplete) {

        var nbAnims = 0;

        var pos = transform.localPosition;
        if (pos.x != snapshot.localPos.x || pos.z != snapshot.localPos.z) {
            throw new NotImplementedException("Animating move and jump not yet implemented");
        }

        if (pos.y != snapshot.localPos.y) {

            nbAnims++;

            //fall of move up
            if (pos.y > snapshot.localPos.y) {
                TryFall((int)(pos.y - snapshot.localPos.y), durationSec);
            } else {
                TryMoveUp((int)(snapshot.localPos.y - pos.y), durationSec);
            }
        }

        if (transform.localRotation != snapshot.localRot) {
            throw new NotImplementedException("Animating rotation not yet implemented");
        }

        if (TryGetComponent<AgeBehavior>(out var ageBehavior)) {

            var ageParadoxBehavior = ageBehavior as AgeParadoxBehavior;

            if (ageBehavior.CurrentAge != snapshot.age) {

                nbAnims++;

                //animate age without paradox
                if (ageParadoxBehavior == null) {
                    ageBehavior.SetCurrentAge(snapshot.age, true, durationSec);
                } else {
                    ageParadoxBehavior.OverrideCurrentAge(false, snapshot.age, true, durationSec);
                }
            }

            //set paradox manually
            if (ageParadoxBehavior != null) {

                ageParadoxBehavior.ClearParadoxState();

                if (snapshot.isInParadoxState) {
                    ageParadoxBehavior.ShowParadoxMesh(snapshot.ageInParadox);
                }
            }
        }

        if (onComplete != null) {
            StartCoroutine(CompleteAnimateChangesAfterDelay(durationSec, onComplete));
        }

        return nbAnims > 0;
    }

    IEnumerator CompleteAnimateChangesAfterDelay(float durationSec, Action onComplete) {

        yield return new WaitForSeconds(durationSec + ANIM_WAIT_BEFORE_CALLBACK_SEC);

        onComplete?.Invoke();
    }

}


public class DisplayableCharacteristics {

    public Color PanelColor { get; private set; }
    public string TextElementTitle { get; private set; }
    public string TextCharacteristics { get; private set; }

    public DisplayableCharacteristics(Color panelColor, string textElementTitle, string textCharacteristics) {
        PanelColor = panelColor;
        TextElementTitle = textElementTitle;
        TextCharacteristics = textCharacteristics;
    }

}
