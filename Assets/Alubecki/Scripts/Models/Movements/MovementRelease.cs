using System;
using System.Collections.Generic;
using UnityEngine;


public class MovementRelease : BaseMovement {


    public override string DisplayableName => "Release";
    public override bool NeedsMovementResolving => false;


    public MovementRelease(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(originalFactory, movementType, owner, nextPos) {
    }

    public override IEnumerable<DisplayableMovementInfo> NewDisplayableMovementInfos() {

        return new List<DisplayableMovementInfo>() {
            new DisplayableMovementInfo(
                MovementDisplay.BUTTON,
                NextPos + Vector3.up,
                0,
                this
            )
        };
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        (owner as CharacterBehavior)?.ReleaseMovableObject(true, onComplete);
    }


#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {

        public override MovementType MovementType => MovementType.ACTION_CHANGE_STATE;

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            var character = owner as CharacterBehavior;

            var movableObject = character.GrabbedMovableObject;
            if (movableObject == null) {
                //no object is grabbed to push/pull it
                return null;
            }

            return new List<Vector3>() { movableObject.GridPos };
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementRelease(this, MovementType, owner, nextPos);
        }

    }

}
