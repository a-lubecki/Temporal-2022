using System;
using System.Collections.Generic;
using UnityEngine;


public class MovementSimpleMove : BaseMovement {


    public const float DURATION_ANIM_MOVE_SEC = 0.2f;
    public const float DURATION_ANIM_AUTOROTATE_SEC = 0.1f;


    public override bool NeedsMovementResolving => true;


    public MovementSimpleMove(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(originalFactory, movementType, owner, nextPos) {
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

        //reveal or hide for NPCs if enter or leave the board
        bool wasInLimits = Game.Instance.boardBehavior.IsInsideBoardHorizontalLimits(owner.GridPos);
        bool willbeInLimits = Game.Instance.boardBehavior.IsInsideBoardHorizontalLimits(NextPos);

        if (!wasInLimits && willbeInLimits) {
            //reveal before anim
            owner.SetMeshesVisible(true);
        }

        bool ok = owner.TryMove(
            NextPos,
            DURATION_ANIM_MOVE_SEC,
            () => {

                if (wasInLimits && !willbeInLimits) {
                    //hide after anim
                    owner.SetMeshesVisible(false);
                }

                onComplete?.Invoke();
            },
            true,
            DURATION_ANIM_AUTOROTATE_SEC
        );
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

            foreach (var orientation in orientations) {

                var nextPos = owner.GetNextGridPos(orientation);
                if (Game.Instance.boardBehavior.IsWalkablePos(nextPos, owner.CanMoveOverInvisibleBlocks)) {
                    res.Add(nextPos);
                }
            }

            return res;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementSimpleMove(this, MovementType, owner, nextPos);
        }

    }

}
