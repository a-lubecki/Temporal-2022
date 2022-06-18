using System;
using UnityEngine;


public class MovementJumpHigh : MovementClimb {


    public override string DisplayableName => IsClimbingUp ? "Jump up" : "Jump down";


    public MovementJumpHigh(MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(movementType, owner, nextPos) {
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        (owner as CharacterBehavior)?.TryJump(NextPos, 0.25f, onComplete, true, 0.1f);
    }


#pragma warning disable 0108
    public class Factory : MovementClimb.Factory {

        protected override float JumpMultiplier => 2;

        protected override bool CanDoAction(CharacterBehavior character) {
            return character.CanJumpHigh;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementJumpHigh(MovementType, owner, nextPos);
        }

    }

}
