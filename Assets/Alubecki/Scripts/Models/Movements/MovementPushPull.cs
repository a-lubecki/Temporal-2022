using System;
using System.Collections.Generic;
using UnityEngine;


public class MovementPushPull : BaseMovement {


    const float DURATION_ANIM_MOVE_SEC = 0.5f;


    public override bool NeedsMovementResolving => true;

    Vector3 nextMovableObjectPos;


    public MovementPushPull(MovementType movementType, BaseElementBehavior owner, Vector3 nextPos, Vector3 nextMovableObjectPos) : base(movementType, owner, nextPos) {
        this.nextMovableObjectPos = nextMovableObjectPos;
    }

    public override IEnumerable<DisplayableMovementInfo> NewDisplayableMovementInfos() {

        var orientation = OrientationFunctions.FindOrientation(Owner.GridPos, NextPos);

        var res = new List<DisplayableMovementInfo>() {
            new DisplayableMovementInfo(
                MovementDisplay.BLOCK,
                nextMovableObjectPos,
                orientation,
                this
            )
        };

        //only when pulling, display a square behind the character
        if (Owner.GridPos == nextMovableObjectPos) {

            res.Add(new DisplayableMovementInfo(
                MovementDisplay.SQUARE,
                NextPos,
                orientation,
                this
            ));
        }

        return res;
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        var character = owner as CharacterBehavior;

        var movableObject = character.GrabbedMovableObject;
        if (movableObject == null) {
            //no object is grabbed to push/pull it
            onComplete?.Invoke();
            return;
        }

        if (!character.CanPushOrPullMovableObject(movableObject)) {
            character.FailPushPull(character.GridPos, DURATION_ANIM_MOVE_SEC, onComplete);
            return;
        }

        movableObject.TryMove(nextMovableObjectPos, DURATION_ANIM_MOVE_SEC);

        //move all objects over the current
        foreach (var elem in Game.Instance.boardBehavior.GetSortedElementsAbove(movableObject)) {

            if (elem is MovableObjectBehavior || elem is CharacterBehavior) {
                var nextPos = new Vector3(nextMovableObjectPos.x, elem.GridPosY, nextMovableObjectPos.z);
                elem.TryMove(nextPos, DURATION_ANIM_MOVE_SEC, null);
            }
        }

        character.TryPushPull(NextPos, DURATION_ANIM_MOVE_SEC, onComplete);
    }


#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {

        public override MovementType MovementType => MovementType.SIMPLE_MOVE;

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            if (owner is not CharacterBehavior) {
                throw new ArgumentException("Only MovableObjects can be pushed/pull");
            }

            CharacterBehavior character = owner as CharacterBehavior;
            if (!character.CanPushOrPull) {
                return null;
            }

            var movableObject = character.GrabbedMovableObject;
            if (movableObject == null) {
                //no object is grabbed to push/pull it
                return null;
            }

            if (!character.IsHorizontallyAdjacentTo(movableObject.GridPos)) {
                return null;
            }

            if (character.GridPosY != movableObject.GridPosY) {
                //not aligned vertically
                return null;
            }

            var res = new List<Vector3>();

            var posPull = character.GridPos + character.GridPos - movableObject.GridPos;//add inverse diff
            var posMovableObjectAfterPush = GetMovableObjectPosAfterPush(character.GridPos, movableObject.GridPos);

            var board = Game.Instance.boardBehavior;

            //check if character can pull behind him
            if (board.IsWalkablePos(posPull)) {
                res.Add(posPull);
            }

            //only check if an object is already behind the movable object
            //don't check if there is something under because the object can fall, except for limits
            if (board.IsInsideBoardHorizontalLimits(posMovableObjectAfterPush) &&
                (!board.HasElemOnPos(posMovableObjectAfterPush) || board.IsWalkablePos(posMovableObjectAfterPush))) {
                res.Add(movableObject.GridPos);
            }

            return res;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {

            CharacterBehavior character = owner as CharacterBehavior;
            var movableObject = character.GrabbedMovableObject;

            var nextMovableObjectPos = (nextPos == movableObject.GridPos) ? GetMovableObjectPosAfterPush(character.GridPos, movableObject.GridPos) : character.GridPos;

            return new MovementPushPull(MovementType, owner, nextPos, nextMovableObjectPos);
        }

        Vector3 GetMovableObjectPosAfterPush(Vector3 characterGridPos, Vector3 movableObjectGridPos) {
            return movableObjectGridPos + movableObjectGridPos - characterGridPos;//add inverse diff
        }

    }

}
