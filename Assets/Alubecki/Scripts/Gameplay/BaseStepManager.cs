using System;
using System.Linq;
using UnityEngine;


public abstract class BaseStepManager : MonoBehaviour {


    protected abstract bool IsCursorMarkerVisible { get; }


    protected virtual void Update() {

        var c = Game.Instance.inGameControlsBehavior;

        if (c.MustPauseGame) {
            PauseGame();
        }

        if (c.MustHideInterface) {
            HideInterface();
        } else if (c.MustShowInterface) {
            ShowInterface();
        }

        if (c.MustGoToPreviousNextMovement) {
            GoToPreviousNextMovement(c.IsPreviousMovement);
        }

        if (c.MustValidateSelection) {
            ValidateSelection(c.PointerPosition);
        }
        if (c.MustNavigatePointer) {
            NavigatePointer(c.PointerPosition);
        }
    }

    void PauseGame() {
        //TODO
    }

    void HideInterface() {
        //TODO
    }

    void ShowInterface() {
        //TODO
    }

    void GoToPreviousNextMovement(bool isPreviousMovement) {

        Game.Instance.gameManager.TryUndoRedoMovement(isPreviousMovement);
    }

    protected void NavigatePointer(Vector2 pointerPosition) {

        var hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(pointerPosition), 200);
        Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        var gridPosBehaviors = hits.Where(h => h.transform.TryGetComponent<GridPosBehavior>(out _));
        if (gridPosBehaviors.Count() <= 0) {
            Game.Instance.elementsSelectionBehavior.NavigateCursorOnPileOfElement(null, false);
        } else {
            var gridPosBehavior = gridPosBehaviors.First().transform.GetComponent<GridPosBehavior>();
            Game.Instance.elementsSelectionBehavior.NavigateCursorOnPileOfElement(gridPosBehavior, IsCursorMarkerVisible);
        }
    }

    protected abstract void ValidateSelection(Vector2 pointerPosition);

}
