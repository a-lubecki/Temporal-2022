using UnityEngine;


public class ViewGameOverBehavior : BaseViewOverlay {


    void Update() {

        if (!IsFullyVisible) {
            //can trigger undi if view is fully visible
            return;
        }

        var c = Game.Instance.inGameControlsBehavior;

        if (c.MustGoToPreviousNextMovement && c.IsPreviousMovement) {
            Game.Instance.gameManager.TryUndoRedoMovement(true);
        }
    }

}
