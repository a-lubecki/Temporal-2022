using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterBehavior))]
public class AINPCBehavior : MonoBehaviour {


    CharacterBehavior characterBehavior;
    [SerializeField] BaseAIDecider decider;

    List<BaseMovement> preparedMovements = new List<BaseMovement>();

    BaseMovement preparedAttack;


    void Awake() {
        characterBehavior = GetComponent<CharacterBehavior>();
    }

    public void InitAIDecider(BaseAIDecider decider) {
        this.decider = decider;
    }

    public int GetPriority() {
        return 1;//TODO get priority to decide what NPC in a team must move first if number of movements are limited during a turn
    }

    public void ResetNextMovements() {

        preparedMovements.Clear();
    }

    public void PrepareNextBestMovement() {

        if (decider == null) {
            return;
        }

        //let the decider decide a movement even if it's sleeping >=> sleeping state check is in ExtractNextPreparedMovement(), after the player movement is resolved
        var nextMovement = decider.NewMovement(characterBehavior);
        if (nextMovement != null) {
            preparedMovements.Add(nextMovement);
        }
    }

    public BaseMovement ExtractNextPreparedMovement() {

        //check if the decider if sleeping now as the board could change between PrepareNextBestMovement and now
        if (decider == null || decider.IsSleeping) {
            return null;
        }

        var movement = preparedMovements.FirstOrDefault();
        if (movement == null) {
            return null;
        }

        preparedMovements.RemoveAt(0);

        return movement;
    }

    public bool CanAttack() {
        return preparedAttack != null;
    }

    public bool TryPreparingAttack() {

        preparedAttack = null;

        var possibleMovement = characterBehavior.GetPossibleMovements()
            .OfType<MovementAttack.Factory>()
            .FirstOrDefault();

        if (possibleMovement == null) {
            //no attack movement
            return false;
        }

        var possibleTargets = possibleMovement.GetNextPossibleMovementTargets(characterBehavior);
        if (possibleTargets == null || possibleTargets.Count() <= 0) {
            //no target pos with another character to attack
            return false;
        }

        preparedAttack = possibleMovement?.NewMovement(characterBehavior, possibleTargets.First());

        return true;
    }

    public void TryExecuteAttack(Action onComplete) {

        if (preparedAttack == null) {
            return;
        }

        preparedAttack?.Execute(onComplete);

        preparedAttack = null;
    }

}
