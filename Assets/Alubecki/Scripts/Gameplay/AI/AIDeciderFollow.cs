using System.Linq;
using UnityEngine;


public class AIDeciderFollow : BaseAIDecider {


    [SerializeField] Transform trTarget;


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

        //only get possible moves on x and z axis
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

        return bestMovement;
    }

    /// <summary>
    /// Simple distance calculation for the demo.
    /// Should be replaced by a real pathfinding algo in the final game.
    /// </summary>
    float CalculateMinDistanceToTarget(Vector2 origin, Vector2 target) {
        return Vector3.Distance(origin, target);
    }

}
