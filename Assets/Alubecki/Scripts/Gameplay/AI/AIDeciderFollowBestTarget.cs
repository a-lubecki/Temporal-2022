using UnityEngine;


public class AIDeciderFollowBestTarget : AIDeciderFollow {


    [SerializeField] Transform[] trPossibleTargets;


    public override BaseMovement NewMovement(CharacterBehavior character) {

        trTarget = FindBestTarget(character);

        return base.NewMovement(character);
    }

    Transform FindBestTarget(CharacterBehavior character) {

        if (trPossibleTargets == null) {
            return null;
        }

        var characterPos = new Vector2(character.GridPosX, character.GridPosZ);

        Transform bestTarget = null;
        var minDistance = float.MaxValue;

        foreach (var t in trPossibleTargets) {

            //pathfinding
            var dist = CalculateMinDistanceToTarget(characterPos, new Vector2(t.position.x, t.position.z));
            if (dist < minDistance) {
                minDistance = dist;
                bestTarget = t;
            }
        }

        return bestTarget;
    }

}
