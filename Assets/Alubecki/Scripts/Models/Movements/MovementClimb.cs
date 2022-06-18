using System;
using System.Collections.Generic;
using UnityEngine;


public class MovementClimb : MovementSimpleMove {


    public bool IsClimbingUp { get; private set; }

    public override string DisplayableName => IsClimbingUp ? "Climb up" : "Climb down";


    public MovementClimb(MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(movementType, owner, nextPos) {
        this.IsClimbingUp = owner.GridPosY < nextPos.y;
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        (owner as CharacterBehavior)?.TryClimb(NextPos, 0.25f, onComplete, true, 0.1f);
    }


#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {

        public override MovementType MovementType => MovementType.SIMPLE_MOVE;

        protected virtual float JumpMultiplier => 1;

        protected virtual bool CanDoAction(CharacterBehavior character) {
            return character.CanClimb;
        }

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            if (!CanDoAction(owner as CharacterBehavior)) {
                return null;
            }

            var res = new List<Vector3>();
            var orientations = Enum.GetValues(typeof(Orientation)) as IEnumerable<Orientation>;

            var board = Game.Instance.boardBehavior;

            foreach (Orientation orientation in orientations) {

                var nextHorizontalPos = owner.GetNextGridPos(orientation);
                var nextPosUp = nextHorizontalPos + JumpMultiplier * Vector3.up;
                var nextPosDown = nextHorizontalPos + JumpMultiplier * Vector3.down;

                if (board.IsWalkablePos(nextPosUp)) {
                    res.Add(nextPosUp);
                } else if (board.IsWalkablePos(nextPosDown)) {
                    res.Add(nextPosDown);
                }
            }

            return res;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementClimb(MovementType, owner, nextPos);
        }

    }

}
