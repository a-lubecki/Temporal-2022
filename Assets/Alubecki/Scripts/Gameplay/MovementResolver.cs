using System;
using System.Collections;
using UnityEngine;


public class MovementResolver : MonoBehaviour {


    public bool IsResolvingMovement { get; private set; }


    public void InitLevel(Action onComplete) {

        //resolve gravity and temporality
        StartCoroutine(ResolveMovementConsequences(onComplete));
    }

    public void ResolveCurrentMovement() {

        IsResolvingMovement = true;

        var nextMovement = Game.Instance.movementsSelectionBehavior.NextMovement;
        if (nextMovement == null) {
            OnMovementResolved();
            return;
        }

        //clear before resolve to avoid bugs
        Game.Instance.movementsSelectionBehavior.ClearNextMovement();

        //execute current player character movmement then resolve it if necessary
        nextMovement.Execute(() => {

            if (!nextMovement.NeedsMovementResolving) {
                OnMovementResolved();
                return;
            }

            StartCoroutine(ResolveMovementConsequences(OnMovementResolved));
        });
    }

    void OnMovementResolved() {
        //triggers the next state change in the visual graph
        IsResolvingMovement = false;
    }

    IEnumerator ResolveMovementConsequences(Action onComplete) {

        yield return ResolveGravity();

        yield return Game.Instance.temporalityManager.ResolveTemporality();

        yield return ResolveGravity();

        var nextNPCMovement = GetNextNPCMovement();
        if (nextNPCMovement == null) {
            onComplete();
            yield break;
        }

        //free the movement to get anothe NPC movement to execute next time
        ClearNPCMovement(nextNPCMovement);

        //execute movement and convert execute callback in "coroutine wait" to stay in curent coroutine
        var isMovementExecuted = false;
        nextNPCMovement.Execute(() => isMovementExecuted = true);

        yield return new WaitUntil(() => isMovementExecuted);

        //resolve attacks, it can lead to game over
        while (IsRemainingAnyNPCAttack()) {

            yield return ResolveNextNPCAttack();

            if (IsAnyPlayerCharacterDefinitelyDead()) {

                Game.Instance.gameManager.TriggerEnd(true);
                onComplete();

                yield break;
            }
        }

        //continue resolving until there are no remaining NPC to move
        yield return ResolveMovementConsequences(onComplete);

        DisplayAllNextNPCMovements();

        onComplete();
    }

    IEnumerator ResolveGravity() {

        var board = Game.Instance.boardBehavior;

        var fallingElemsCount = 0;

        foreach (var elem in board.GetAboutToFallElements()) {

            var fallHeight = board.GetFallHeight(elem);
            var durationSec = 0.4f * fallHeight;

            fallingElemsCount++;
            elem.TryFall(fallHeight, durationSec, () => fallingElemsCount--);

            foreach (var supElem in board.GetSortedElementsAbove(elem)) {

                fallingElemsCount++;
                supElem.TryFall(fallHeight, durationSec, () => fallingElemsCount--);
            }
        }

        yield return new WaitWhile(() => fallingElemsCount > 0);
        yield return new WaitForSeconds(0.01f);
    }

    BaseMovement GetNextNPCMovement() {
        return null;//TODO
    }

    void ClearNPCMovement(BaseMovement movement) {
        //TODO
    }

    bool IsRemainingAnyNPCAttack() {
        return false;//TODO
    }

    IEnumerator ResolveNextNPCAttack() {

        yield break;//TODO
    }

    bool IsAnyPlayerCharacterDefinitelyDead() {
        return false;//TODO
    }

    void DisplayAllNextNPCMovements() {
        //TODO
    }

}
