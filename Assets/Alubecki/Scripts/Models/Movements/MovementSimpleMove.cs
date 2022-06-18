using System;
using System.Collections.Generic;
using UnityEngine;


public class MovementSimpleMove : BaseMovement {


    public override bool NeedsMovementResolving => true;


    public MovementSimpleMove(MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(movementType, owner, nextPos) {
    }

    public override IEnumerable<DisplayableMovementInfo> NewDisplayableMovementInfos() {

        return new List<DisplayableMovementInfo>() {
            new DisplayableMovementInfo(
                MovementDisplay.SQUARE,
                NextPos,
                OrientationFunctions.FindOrientation(Owner.GridPos, NextPos),
                this
            )
        };
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        owner.TryMove(NextPos, 0.25f, onComplete, true, 0.1f);
    }


#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {

        public override MovementType MovementType => MovementType.SIMPLE_MOVE;

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            if (!owner.CanMove) {
                return null;
            }

            var res = new List<Vector3>();
            var orientations = Enum.GetValues(typeof(Orientation)) as IEnumerable<Orientation>;

            foreach (Orientation orientation in orientations) {

                var nextPos = owner.GetNextGridPos(orientation);
                if (Game.Instance.boardBehavior.IsWalkablePos(nextPos)) {
                    res.Add(nextPos);
                }
            }

            return res;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementSimpleMove(MovementType, owner, nextPos);
        }

    }

}
