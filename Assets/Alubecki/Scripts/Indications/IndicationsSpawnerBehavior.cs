using System;
using Lean.Pool;
using UnityEngine;


public class IndicationsSpawnerBehavior : MonoBehaviour {


    [SerializeField] Transform trIndicationsCharacterMove;
    [SerializeField] Transform trIndicationsMovableObjectMove;
    [SerializeField] Transform trIndicationsNPCAction;

    public void SpawnMovementIndication(DisplayableMovementInfo info) {

        LeanGameObjectPool pool;
        Transform trParent;

        //, info.pos, Quaternion.Euler(0, info.Orientation, 0)
        if (info.Display == MovementDisplay.SQUARE) {

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

}