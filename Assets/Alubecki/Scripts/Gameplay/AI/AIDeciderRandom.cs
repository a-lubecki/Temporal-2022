using System.Linq;
using UnityEngine;


public class AIDeciderRandom : BaseAIDecider {

    public override BaseMovement NewMovement(CharacterBehavior character) {

        var possibleMovements = character.GetPossibleMovements();
        if (possibleMovements == null || possibleMovements.Count() <= 0) {
            return null;
        }

        var hasFoundTargetPos = false;
        BaseMovement.Factory foundPossibleMovement = null;
        Vector3 target = default;

        var randomPossibleMovements = possibleMovements.ToList();
        randomPossibleMovements.Shuffle();

        foreach (var possibleMovement in randomPossibleMovements) {

            foundPossibleMovement = possibleMovement;

            var targets = possibleMovement.GetNextPossibleMovementTargets(character);
            if (targets != null && targets.Count() > 0) {

                hasFoundTargetPos = true;
                target = targets.ToList().PickRandom();

                break;
            }
        }

        if (!hasFoundTargetPos) {
            return null;
        }

        return foundPossibleMovement.NewMovement(character, target);
    }

}
