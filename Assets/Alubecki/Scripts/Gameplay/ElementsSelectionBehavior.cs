using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ElementsSelectionBehavior : MonoBehaviour {


    [SerializeField] AudioClip audioClipCursorMove;
    [SerializeField] AudioClip audioClipSelect;
    [SerializeField] AudioClip audioClipDeselect;

    /// <summary>
    /// Pile of elements ordered from bottom to top
    /// </summary>
    private IEnumerable<BaseElementBehavior> currentPile;

    public BaseElementBehavior LastSelectedElement { get; private set; }
    public BaseElementBehavior SelectedElement { get; private set; }

    public bool IsElementSelected => SelectedElement != null;
    public bool IsNewElementSelected => SelectedElement != null && LastSelectedElement != null && SelectedElement != LastSelectedElement;


    public void NavigateCursorOnPileOfElement(GridPosBehavior gridPosBehavior, bool hasMarker) {

        if (gridPosBehavior == null) {
            HideCursor();
            return;
        }

        var horizontalPos = new Vector2(gridPosBehavior.GridPosX, gridPosBehavior.GridPosZ);

        currentPile = Game.Instance.boardBehavior.GetSortedPileOfElements(horizontalPos);

        var nextPos = new Vector3(
            horizontalPos.x,
            GetCursorVerticalPosOnPile(),
            horizontalPos.y
        );

        //only need to play sound if was hidden or if position is different
        var mustPlaySound = (!Game.Instance.cursorBehavior.IsShown || !Game.Instance.cursorBehavior.IsOnGridPos(nextPos));

        var markerDisplay = CursorMarkerDisplay.HIDDEN;
        if (hasMarker) {
            var lastElem = currentPile?.LastOrDefault();
            if (lastElem != null) {
                markerDisplay = lastElem.IsCursorAboveElement ? CursorMarkerDisplay.BOTTOM : CursorMarkerDisplay.TOP;
            }
        }

        Game.Instance.cursorBehavior.Show(nextPos, markerDisplay);
        Game.Instance.panelPileBehavior.Show(currentPile);

        if (mustPlaySound) {
            Game.Instance.audioManager.PlaySimpleSound(audioClipCursorMove);
        }
    }

    public void HideCursor() {

        //if no elements on this pos, no need to show cursor
        currentPile = null;
        Game.Instance.cursorBehavior.Hide();

        Game.Instance.panelPileBehavior.Hide();
    }

    int GetCursorVerticalPosOnPile() {

        var lastElem = currentPile?.LastOrDefault();
        if (lastElem == null) {
            return 0;
        }

        if (lastElem.IsCursorAboveElement) {
            return lastElem.GridPosY + lastElem.ColliderHeight;
        }

        return lastElem.GridPosY;
    }

    public void ValidateSelection(BaseElementBehavior selectableElem, bool canPlaySound = true) {

        if (SelectedElement != null && selectableElem == SelectedElement) {
            //elem already selected
            return;
        }

        LastSelectedElement = SelectedElement;
        SelectedElement = selectableElem;

        //play select or deselect audio
        if (canPlaySound && SelectedElement != LastSelectedElement) {
            var clip = SelectedElement != null ? audioClipSelect : audioClipDeselect;
            Game.Instance.audioManager.PlaySimpleSound(clip);
        }
    }

    public void CancelSelection(bool canPlaySound = true) {
        ValidateSelection(null, canPlaySound);
    }

    public void ClearLastSelected() {
        LastSelectedElement = null;
    }

}
