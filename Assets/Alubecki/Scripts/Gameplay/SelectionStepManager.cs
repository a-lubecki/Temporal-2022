using System;
using System.Linq;
using UnityEngine;


public class SelectionStepManager : BaseStepManager {


    protected override bool IsCursorMarkerVisible => true;


    protected override void ValidateSelection(Vector2 pointerPosition) {

        var hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(pointerPosition), 200);
        Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        //selection on playable characters (raycast through elements is possible)
        var elems = hits.Where(h => h.transform.TryGetComponent<BaseElementBehavior>(out var e) && e.CanBeSelected);
        if (elems.Count() <= 0) {
            return;
        }

        var elem = elems.First().transform.GetComponent<BaseElementBehavior>();
        Game.Instance.elementsSelectionBehavior.ValidateSelection(elem);
    }

}
