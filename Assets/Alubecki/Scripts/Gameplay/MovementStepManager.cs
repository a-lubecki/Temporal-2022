using System;
using System.Linq;
using UnityEngine;


public class MovementStepManager : BaseStepManager {


    protected override bool IsCursorMarkerVisible => false;


    protected override void Update() {
        base.Update();

        var c = Game.Instance.inGameControlsBehavior;

        if (c.MustCancelSelection) {
            CancelSelection();
        }
    }

    protected override void ValidateSelection(Vector2 pointerPosition) {

        var hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(pointerPosition), 200);
        Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        if (hits.Count() <= 0) {
            return;
        }

        //selection on an indication square
        var indications = hits.Where(h => h.transform.TryGetComponent<IndicationMoveBehavior>(out var e));
        if (indications.Count() > 0) {
            var indication = indications.First().transform.GetComponent<IndicationMoveBehavior>();
            Game.Instance.movementsSelectionBehavior.ValidateNextMovement(indication.Movement);
            return;
        }

        //selection on playable characters (raycast through elements is possible)
        var elems = hits.Where(h => h.transform.TryGetComponent<BaseElementBehavior>(out var e) && e.CanBeSelected);
        if (elems.Count() > 0) {
            var elem = elems.First().transform.GetComponent<BaseElementBehavior>();
            Game.Instance.elementsSelectionBehavior.ValidateSelection(elem);
        }
    }

    void CancelSelection() {

        Game.Instance.elementsSelectionBehavior.CancelSelection();
    }

}
