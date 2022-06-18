using System;
using UnityEngine;


public class ActionButtonsSpawnerBehavior : MonoBehaviour {


    public void SpawnActionButton(Vector3 gridPos, BaseMovement actionOneTime, BaseMovement actionStateChange) {

        if (actionOneTime != null && actionOneTime.MovementType != MovementType.ACTION_ONE_TIME) {
            throw new ArgumentException();
        }
        if (actionStateChange != null && actionStateChange.MovementType != MovementType.ACTION_CHANGE_STATE) {
            throw new ArgumentException();
        }

        var go = Game.Instance.poolActionButtonGroup.Spawn(transform);

        go.GetComponent<ActionButtonGroupBehavior>().UpdateUI(
            gridPos,
            actionOneTime?.Owner.GridPos ?? actionStateChange?.Owner.GridPos ?? Vector3.zero,
            actionOneTime,
            actionStateChange
        );
    }

    public void DespawnAllActionButtons() {

        Game.Instance.poolActionButtonGroup.DespawnAll();
    }

}
