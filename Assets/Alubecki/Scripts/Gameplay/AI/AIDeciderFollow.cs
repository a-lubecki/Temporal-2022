using System.Linq;
using UnityEngine;


public class AIDeciderFollow : BaseAIDecider {


    [SerializeField] protected Transform trTarget;


    public override BaseMovement NewMovement(CharacterBehavior character) {

        if (trTarget == null) {
            return null;
        }

        var targetPos = trTarget.transform.position;

        var characterPos2d = new Vector2(character.GridPosX, character.GridPosZ);
        var targetPos2d = new Vector2(targetPos.x, targetPos.z);

        if (characterPos2d == targetPos2d) {
            //already on pos
            return null;
        }

        var possibleMovementAggregate = character.GetPossibleMovements()
            .FirstOrDefault(m => m is MovementAggregate.Factory);

        if (possibleMovementAggregate == null) {
            Debug.LogWarning("NPC doesn't have a MovementAggregate.Factory, it can't move");
            return null;
        }

        //init with current distance to have a first reference
        var minDistance = float.MaxValue;
        Vector3 bestNextPos = character.GridPos;

        //find closest target
        var possibleTargets = possibleMovementAggregate.GetNextPossibleMovementTargets(character);
        foreach (var pos in possibleTargets) {

            //pathfinding
            var dist = CalculateMinDistanceToTarget(new Vector2(pos.x, pos.z), targetPos2d);

            if (dist > 0 && !Game.Instance.boardBehavior.IsWalkablePos(pos, character.CanMoveOverInvisibleBlocks)) {
                //handle the MovementSimpleLookAt case, to avoid NPC looking at the target behing a wall
                continue;
            }

            if (dist < minDistance) {
                minDistance = dist;
                bestNextPos = pos;
            }
        }

        if (minDistance == float.MaxValue) {
            //no best target pos was found
            Debug.Log("No target found for NPC follow algo");
            return null;
        }

        return possibleMovementAggregate.NewMovement(character, bestNextPos);
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
