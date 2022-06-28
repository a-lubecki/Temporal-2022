using System;
using System.Collections;
using System.Linq;
using UnityEngine;


public class MovementResolver : MonoBehaviour {


    public const float DURATION_ANIM_TEMPORALITY_CHANGE_SEC = 0.1f;


    public bool IsResolvingMovement { get; private set; }

    //a value stored in memory to avoid same computations every time
    bool hasFoundAnyPlayerDefinitelyDead;


    public void InitLevel(Action onComplete) {

        //resolve gravity and temporality
        StartCoroutine(ResolveMovementConsequences(onComplete));
    }

    public void ExecuteCurrentMovement() {

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

    /// <summary>
    /// If any player character dead, invoke oncomplete and return true
    /// </summary>
    bool InterruptIfAnyCharacterDead(Action onComplete) {

        if (hasFoundAnyPlayerDefinitelyDead) {

            Game.Instance.gameManager.TriggerEnd(true);
            onComplete?.Invoke();
            return true;
        }

        return false;
    }

    IEnumerator ResolveMovementConsequences(Action onComplete) {

        //reset cached value
        hasFoundAnyPlayerDefinitelyDead = false;

        yield return ResolveNPCsAttacks();
        if (InterruptIfAnyCharacterDead(onComplete)) {
            yield break;
        }

        yield return ResolveTemporalityAndGravity();
        if (InterruptIfAnyCharacterDead(onComplete)) {
            yield break;
        }

        yield return ResolveNPCsMovements(Game.Instance.aiTeamNeutral);
        if (InterruptIfAnyCharacterDead(onComplete)) {
            yield break;
        }

        yield return ResolveNPCsMovements(Game.Instance.aiTeamEnemy);
        if (InterruptIfAnyCharacterDead(onComplete)) {
            yield break;
        }

        if (AreAllPlayerCharactersOnGoals()) {

            Game.Instance.gameManager.TriggerEnd(false);
            onComplete?.Invoke();

            yield break;
        }

        onComplete?.Invoke();
    }

    IEnumerator ResolveTemporalityAndGravity() {

        var currentSnapshot = Game.Instance.boardBehavior.NewSnapshot();

        Game.Instance.temporalityManager.ComputeTemporalityAndGravity();

        var newSnapshot = Game.Instance.boardBehavior.NewSnapshot();
        Game.Instance.boardBehavior.Restore(currentSnapshot);

        yield return AnimateChangesToSnapshots((MementoSnapshotBoard)newSnapshot);
    }

    IEnumerator AnimateChangesToSnapshots(MementoSnapshotBoard nextSnapshot) {

        int nbAnims = 0;

        //find elemsnts by elem snapshot then animate changes
        nextSnapshot.ProcessElements(
            Game.Instance.boardBehavior.GetElements(),
            (elem, elemSnapshot) => {

                if (elem.AnimateChanges(elemSnapshot, DURATION_ANIM_TEMPORALITY_CHANGE_SEC, () => nbAnims--)) {
                    nbAnims++;
                }
            }
        );

        //wait until all anims have finished
        yield return new WaitWhile(() => nbAnims > 0);
        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator ResolveNPCsMovements(AITeamBehavior aiTeam) {

        var nextNPCMovement = aiTeam.AdvanceToNextAvailableNPCMovement();

        while (nextNPCMovement != null) {

            if (nextNPCMovement.CanExecute()) {

                //execute movement and convert execute callback in "coroutine wait" to stay in curent coroutine
                var isMovementExecuted = false;
                nextNPCMovement.Execute(() => isMovementExecuted = true);

                yield return new WaitUntil(() => isMovementExecuted);

                yield return ResolveTemporalityAndGravity();

                if (IsAnyPlayerCharacterDefinitelyDead()) {
                    //stop now, game over will be triggered outside this method
                    hasFoundAnyPlayerDefinitelyDead = true;
                    yield break;
                }

            } else {

                Debug.Log("NPC movement couldn't be executed: " + nextNPCMovement);
            }

            yield return ResolveNPCsAttacks();
            if (hasFoundAnyPlayerDefinitelyDead) {
                yield break;
            }

            nextNPCMovement = aiTeam.AdvanceToNextAvailableNPCMovement();
        }
    }

    IEnumerator ResolveNPCsAttacks() {

        yield return ResolveNPCsAttacks(Game.Instance.aiTeamEnemy);
        if (hasFoundAnyPlayerDefinitelyDead) {
            yield break;
        }

        yield return ResolveNPCsAttacks(Game.Instance.aiTeamNeutral);
    }

    IEnumerator ResolveNPCsAttacks(AITeamBehavior aiTeam) {

        //reset attack flags before launching algo, flag is useful to avoid infinite algo in certain cases
        aiTeam.ResetNPCAttackFlags();

        //resolve attacks, it can lead to game over
        var attackingNPC = aiTeam.GetRemainingNPCWithAttack();
        while (attackingNPC != null) {

            yield return attackingNPC.Attack();

            if (IsAnyPlayerCharacterDefinitelyDead()) {
                //stop now, game over will be triggered outside this method
                hasFoundAnyPlayerDefinitelyDead = true;
                yield break;
            }

            attackingNPC = aiTeam.GetRemainingNPCWithAttack();
        }
    }

    bool IsAnyPlayerCharacterDefinitelyDead() {

        return hasFoundAnyPlayerDefinitelyDead || Game.Instance.boardBehavior.GetElements()
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

}
