using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovementSimpleLookAt : BaseMovement {


    public const float DURATION_ANIM_ROTATE_SEC = 0.2f;


    public override bool NeedsMovementResolving => true;


    public MovementSimpleLookAt(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(originalFactory, movementType, owner, nextPos) {
    }

    public override IEnumerable<DisplayableMovementInfo> NewDisplayableMovementInfos() {
        return null;//not used
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        owner.TryLookAt(NextPos, DURATION_ANIM_ROTATE_SEC, () => onComplete?.Invoke());
    }


#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {

        public override MovementType MovementType => MovementType.SIMPLE_MOVE;

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            if (!owner.CanMove) {
                return null;
            }

            return (Enum.GetValues(typeof(Orientation)) as IEnumerable<Orientation>)
                .Select(orientation => owner.GetNextGridPos(orientation));
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementSimpleLookAt(this, MovementType, owner, nextPos);
        }

    }

}
