using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovementResolver : MonoBehaviour {


    public bool IsResolvingMovement { get; private set; }


    public void InitLevel(Action onComplete) {

        //resolve gravity and temporality
        StartCoroutine(ResolveMovementConsequences(onComplete));
    }

    public void ResolveCurrentMovement() {

        if (IsResolvingMovement) {
            throw new InvalidOperationException("Already resolving movement");
        }

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

        yield return ResolveTemporalityAndGravity();

        yield return ResolveNPCMovements();

        if (IsAnyPlayerCharacterDefinitelyDead()) {

            Game.Instance.gameManager.TriggerEnd(true);
            onComplete?.Invoke();

            yield break;
        }

        if (AreAllPlayerCharactersOnGoals()) {

            Game.Instance.gameManager.TriggerEnd(false);
            onComplete?.Invoke();

            yield break;
        }

        DisplayAllNextNPCMovements();

        onComplete?.Invoke();
    }

    IEnumerator ResolveTemporalityAndGravity() {

        //TEST METHOD WITHOUT ANIMS
        Game.Instance.temporalityManager.ComputeTemporalityAndGravity();
        yield return new WaitForSeconds(0.1f);
    }
    /*
        IEnumerator ResolveTemporalityAndGravity() {

            var currentSnapshot = Game.Instance.memento.GetCurrentSnapshot();

            Game.Instance.temporalityManager.ComputeTemporalityAndGravity();

            var newSnapshot = Game.Instance.boardBehavior.CreateSnapshot();
            Game.Instance.memento.Restore(currentSnapshot);

            yield return AnimateChangesBetweenSnapshots(currentSnapshot, newSnapshot);
        }*/

    /*
        IEnumerator AnimateChangesBetweenSnapshots(Snapshot previous, Snapshot next) {

            //TODO

            yield return new WaitWhile(() => nbAnims > 0);
            yield return new WaitForSeconds(0.01f);
        }*/

    IEnumerator ResolveNPCMovements() {

        var nextNPCMovement = GetNextNPCMovement();

        while (nextNPCMovement != null) {

            //free the movement to get anothe NPC movement to execute next time
            ClearNPCMovement(nextNPCMovement);

            //execute movement and convert execute callback in "coroutine wait" to stay in curent coroutine
            var isMovementExecuted = false;
            nextNPCMovement.Execute(() => isMovementExecuted = true);

            yield return new WaitUntil(() => isMovementExecuted);

            yield return ResolveTemporalityAndGravity();

            if (IsAnyPlayerCharacterDefinitelyDead()) {
                //stop now, game over will be triggered outside this method
                yield break;
            }

            //resolve attacks, it can lead to game over
            while (IsRemainingAnyNPCAttack()) {

                yield return ResolveNextNPCAttack();

                if (IsAnyPlayerCharacterDefinitelyDead()) {
                    //stop now, game over will be triggered outside this method
                    yield break;
                }
            }

            nextNPCMovement = GetNextNPCMovement();
        }
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

        return Game.Instance.boardBehavior.GetElements()
            .Any(e => e.TryGetComponent<CharacterBehavior>(out var character) && character.IsPlayable && character.IsDefinitelyDead);
    }

    bool AreAllPlayerCharactersOnGoals() {

        return Game.Instance.boardBehavior.GetElements()
            .Where(e => e.TryGetComponent<CharacterBehavior>(out var character) && character.IsPlayable && character.IsAlive)
            .All(e => IsGoalOnPos(e.GridPos));
    }

    bool IsGoalOnPos(Vector3 pos) {

        return Game.Instance.boardBehavior.GetElemsOnPos(pos)
            .Any(e => e.TryGetComponent<GoalBehavior>(out _));
    }

    void DisplayAllNextNPCMovements() {
        //TODO
    }

}
