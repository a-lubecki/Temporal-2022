using System.Linq;
using UnityEngine;


public class AIDeciderFollow : BaseAIDecider {


    [SerializeField] protected Transform trTarget;


    public override BaseMovement NewMovement(CharacterBehavior character) {

        if (trTarget == null) {
            return null;
        }

        var characterPos = new Vector2(character.GridPosX, character.GridPosZ);
        var targetPos = new Vector2(trTarget.transform.position.x, trTarget.transform.position.z);

        if (characterPos == targetPos) {
            //already on pos
            return null;
        }

        //move on x and z axis, if the character can climb on jump, it will be automatically resolved
        var possibleMovements = character.GetPossibleMovements()
            .Where(m => m is MovementSimpleMove.Factory || m is MovementClimb.Factory || m is MovementJumpHigh.Factory);

        if (possibleMovements == null || possibleMovements.Count() <= 0) {
            return null;
        }

        var minDistance = CalculateMinDistanceToTarget(characterPos, targetPos);
        BaseMovement bestMovement = null;

        foreach (var possibleMovement in possibleMovements) {

            var targets = possibleMovement.GetNextPossibleMovementTargets(character);
            if (targets == null) {
                continue;
            }

            foreach (var pos in targets) {

                //pathfinding
                var dist = CalculateMinDistanceToTarget(new Vector2(pos.x, pos.z), targetPos);
                if (dist < minDistance) {
                    minDistance = dist;
                    bestMovement = possibleMovement.NewMovement(character, pos);
                }
            }
        }

        //if near and couldn't walk, look at
        if (bestMovement == null && IsHorizontallyAdjacent(characterPos, targetPos)) {

            var possibleMovementLookAt = character.GetPossibleMovements()
                .FirstOrDefault(m => m is MovementSimpleLookAt.Factory);

            var destinationPos = new Vector3(targetPos.x, character.GridPosY, targetPos.y);
            return possibleMovementLookAt?.NewMovement(character, destinationPos);
        }

        return bestMovement;
    }

    bool IsHorizontallyAdjacent(Vector2 origin, Vector2 target) {
        return origin.x == target.x && Mathf.Abs(origin.y - target.y) == 1
            || origin.y == target.y && Mathf.Abs(origin.x - target.x) == 1;
    }

    /// <summary>
    /// Simple distance calculation for the demo.
    /// Should be replaced by a real pathfinding algo in the final game.
    /// </summary>
    protected float CalculateMinDistanceToTarget(Vector2 origin, Vector2 target) {
        return Vector3.Distance(origin, target);
    }

}
