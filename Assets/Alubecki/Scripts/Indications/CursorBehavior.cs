using UnityEngine;


[RequireComponent(typeof(Animation))]
public class CursorBehavior : GridPosBehavior {


    Animation animationMarkerMove;
    [SerializeField] Transform trSquare;
    [SerializeField] Transform trMarker;

    [SerializeField] Transform trSquareMovement;

    public bool IsShown => gameObject.activeInHierarchy;


    void Awake() {
        animationMarkerMove = GetComponent<Animation>();
    }

    public void Show(Vector3 pos, CursorMarkerDisplay markerDisplay) {

        gameObject.SetActive(true);

        var hasMarker = markerDisplay != CursorMarkerDisplay.HIDDEN;

        trSquare.gameObject.SetActive(hasMarker);
        trMarker.gameObject.SetActive(hasMarker);
        trSquareMovement.gameObject.SetActive(!hasMarker);

        SetGridPos(pos);

        if (markerDisplay == CursorMarkerDisplay.BOTTOM) {
            trMarker.localPosition = Vector3.zero;
        } else if (markerDisplay == CursorMarkerDisplay.TOP) {
            trMarker.localPosition = Vector3.up;
        }

        if (markerDisplay == CursorMarkerDisplay.TOP) {
            animationMarkerMove.Play();
        } else {
            animationMarkerMove.Stop();
        }
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

}


public enum CursorMarkerDisplay {

    HIDDEN,
    BOTTOM,
    TOP
}
