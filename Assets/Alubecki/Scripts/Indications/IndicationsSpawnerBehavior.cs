using System;
using Lean.Pool;
using UnityEngine;


public class IndicationsSpawnerBehavior : MonoBehaviour {


    [SerializeField] Transform trIndicationsCharacterMove;
    [SerializeField] Transform trIndicationsMovableObjectMove;
    [SerializeField] Transform trIndicationsNPCMovement;

    public void SpawnMovementIndication(DisplayableMovementInfo info) {

        LeanGameObjectPool pool;
        Transform trParent;

        var owner = info.Movement?.Owner as CharacterBehavior;
        if (owner != null && owner.IsNPC) {

            pool = Game.Instance.poolIndicationNPCMovement;
            trParent = trIndicationsNPCMovement;

        } else if (info.Display == MovementDisplay.SQUARE) {

            pool = Game.Instance.poolIndicationCharacterMove;
            trParent = trIndicationsCharacterMove;

        } else if (info.Display == MovementDisplay.BLOCK) {

            pool = Game.Instance.poolIndicationMovableObjectMove;
            trParent = trIndicationsMovableObjectMove;

        } else {
            throw new NotImplementedException();
        }

        var go = pool.Spawn(info.Pos, Quaternion.Euler(0, (int)info.Orientation, 0), trParent);
        go.GetComponent<IndicationMoveBehavior>().SetMovement(info.Movement);
    }

    public void DespawnAllMovementIndications() {

        Game.Instance.poolIndicationCharacterMove.DespawnAll();
        Game.Instance.poolIndicationMovableObjectMove.DespawnAll();
    }

    public void DespawnNPCMovementIndication(CharacterBehavior characterBehavior) {

        if (characterBehavior == null) {
            throw new ArgumentException();
        }

        foreach (Transform t in trIndicationsNPCMovement) {

            var indication = t.GetComponent<IndicationMoveBehavior>();
            if (indication.Movement?.Owner == characterBehavior) {
                Game.Instance.poolIndicationNPCMovement.Despawn(indication.gameObject);
                return;
            }
        }
    }

}