using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovementGrab : BaseMovement {


    public override string DisplayableName => "Grab";
    public override bool NeedsMovementResolving => false;


    public MovementGrab(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(originalFactory, movementType, owner, nextPos) {
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

        var movableObject = Game.Instance.boardBehavior.GetElemsOnPos(NextPos).FirstOrDefault(e => e is MovableObjectBehavior);
        if (movableObject == null) {
            onComplete?.Invoke();
            return;
        }

        (owner as CharacterBehavior)?.GrabMovableObject(movableObject as MovableObjectBehavior, true, onComplete, MovementSimpleMove.DURATION_ANIM_AUTOROTATE_SEC);
    }


#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {

        public override MovementType MovementType => MovementType.ACTION_CHANGE_STATE;

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            var character = owner as CharacterBehavior;
            if (!character.CanGrab) {
                return null;
            }

            var res = new List<Vector3>();
            var orientations = Enum.GetValues(typeof(Orientation)) as IEnumerable<Orientation>;

            foreach (Orientation orientation in orientations) {

                var nextPos = owner.GetNextGridPos(orientation);
                var movableObject = Game.Instance.boardBehavior.GetElemsOnPos(nextPos).FirstOrDefault(e => e is MovableObjectBehavior) as MovableObjectBehavior;
                if (movableObject != null && movableObject.CanBeGrabbed) {
                    res.Add(nextPos);
                }
            }

            return res;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementGrab(this, MovementType, owner, nextPos);
        }

    }

}
