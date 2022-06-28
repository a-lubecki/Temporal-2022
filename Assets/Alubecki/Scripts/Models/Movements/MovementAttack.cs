using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovementAttack : BaseMovement {


    public override bool NeedsMovementResolving => true;


    public MovementAttack(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) : base(originalFactory, movementType, owner, nextPos) {
    }

    public override IEnumerable<DisplayableMovementInfo> NewDisplayableMovementInfos() {
        return null;//TODO for player characters
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        var character = (owner as CharacterBehavior);

        if (character == null || !FindAttackedPos(character, NextPos, out var foundPos)) {
            onComplete?.Invoke();
            return;
        }

        character.Attack(
            foundPos,
            MovementSimpleMove.DURATION_ANIM_AUTOROTATE_SEC,
            () => {

                //resolve attacked characters
                foreach (var c in GetCharactersOnAttackPos(character, foundPos)) {
                    c.SetAsDead();
                }

                onComplete?.Invoke();
            }
        );
    }

    /// <summary>
    /// Character can attack on pos, on pos 1 block upper or on pos 1 block lower
    /// </summary>
    static bool FindAttackedPos(CharacterBehavior character, Vector3 nextPos, out Vector3 foundPos) {

        var possiblePositions = new Vector3[] {
             nextPos,
             nextPos + Vector3.up,
             nextPos + Vector3.down
        };

        foreach (var pos in possiblePositions) {

            if (GetCharactersOnAttackPos(character, pos).Count() > 0) {
                foundPos = pos;
                return true;
            }
        }

        foundPos = default;
        return false;
    }

    static IEnumerable<CharacterBehavior> GetCharactersOnAttackPos(CharacterBehavior character, Vector3 pos) {

        var team = character.Team;

        return Game.Instance.boardBehavior.GetElemsOnPos(pos)
            .OfType<CharacterBehavior>()
            .Where(c => !c.IsInvisible && IsAttackMatrixSatisfied(team, c.Team));
    }

    static bool IsAttackMatrixSatisfied(Team attacker, Team attacked) {

        return attacker == Team.ALLY && attacked == Team.ENEMY ||
            attacker == Team.NEUTRAL && attacked == Team.ENEMY ||
            attacker == Team.ENEMY && attacked == Team.ALLY ||
            attacker == Team.ENEMY && attacked == Team.NEUTRAL;
    }



#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {

        public override MovementType MovementType => MovementType.ACTION_ONE_TIME;

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            if (!owner.CanMove) {
                return null;
            }

            if (owner.IsInvisible) {
                //can't attack if not on the board
                return null;
            }

            var character = (owner as CharacterBehavior);

            if (!character.IsEnemy) {
                //temporary test for demo: only enemy can attack
                return null;
            }

            var res = new List<Vector3>();
            var orientations = Enum.GetValues(typeof(Orientation)) as IEnumerable<Orientation>;

            foreach (var orientation in orientations) {

                var nextPos = owner.GetNextGridPos(orientation);
                if (FindAttackedPos(character, nextPos, out _)) {
                    //found another character to attack
                    res.Add(nextPos);
                }
            }

            return res;
        }

        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {
            return new MovementAttack(this, MovementType, owner, nextPos);
        }

    }

}
