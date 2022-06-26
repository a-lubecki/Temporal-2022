using System;
using UnityEngine;


public class MovementJumpHigh : MovementClimb {


    public override string DisplayableName => IsClimbingUp ? "Jump up" : "Jump down";


    public MovementJumpHigh(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(originalFactory, movementType, owner, nextPos) {
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {
        //replace movement: no base method call
        (owner as CharacterBehavior)?.TryJump(NextPos, DURATION_ANIM_CLIMB_SEC, onComplete, true, DURATION_ANIM_AUTOROTATE_SEC);
    }


#pragma warning disable 0108
    public class Factory : MovementClimb.Factory {

        protected override float JumpMultiplier => 2;

        protected override bool CanDoAction(CharacterBehavior character) {
            return character.CanJumpHigh;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementJumpHigh(this, MovementType, owner, nextPos);
        }

    }

}
