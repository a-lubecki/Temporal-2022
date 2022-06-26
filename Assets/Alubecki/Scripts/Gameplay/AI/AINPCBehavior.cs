using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterBehavior))]
public class AINPCBehavior : MonoBehaviour {


    CharacterBehavior characterBehavior;
    [SerializeField] BaseAIDecider decider;

    List<BaseMovement> preparedMovements = new List<BaseMovement>();
    bool didAttack;


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

    public void ResetAttackFlag() {

        didAttack = false;
    }

    public bool CanAttack() {

        if (didAttack) {
            //can't do several attacks
            return false;
        }

        if (characterBehavior.IsInvisible) {
            //can't attack if not on the board
            return false;
        }

        if (!characterBehavior.IsEnemy) {
            //temporary test for demo
            return false;
        }

        return GetCharactersOnAttackPos().Count() > 0;
    }

    Vector3 GetAttackPos() => characterBehavior.Orientation switch {

        Orientation.North => characterBehavior.GridPos + Vector3.forward,
        Orientation.East => characterBehavior.GridPos + Vector3.right,
        Orientation.South => characterBehavior.GridPos + Vector3.back,
        Orientation.West => characterBehavior.GridPos + Vector3.left,
        _ => throw new NotImplementedException()
    };

    IEnumerable<CharacterBehavior> GetCharactersOnAttackPos() {

        return Game.Instance.boardBehavior.GetElemsOnPos(GetAttackPos())
            .OfType<CharacterBehavior>()
            .Where(c => !c.IsInvisible && IsAttackMatrixSatisfied(characterBehavior.Team, c.Team));
    }

    bool IsAttackMatrixSatisfied(Team attacker, Team attacked) {

        return attacker == Team.ALLY && attacked == Team.ENEMY ||
            attacker == Team.NEUTRAL && attacked == Team.ENEMY ||
            attacker == Team.ENEMY && attacked == Team.ALLY ||
            attacker == Team.ENEMY && attacked == Team.NEUTRAL;
    }

    public IEnumerator Attack() {

        didAttack = true;

        var isAnimating = true;

        characterBehavior.Attack(
            GetAttackPos(),
            MovementSimpleMove.DURATION_ANIM_AUTOROTATE_SEC,
            () => isAnimating = false
        );

        yield return new WaitWhile(() => isAnimating);

        //resolve attacked characters
        var attackedCharacters = GetCharactersOnAttackPos();
        foreach (var c in attackedCharacters) {
            c.SetAsDead();
        }
    }

}
