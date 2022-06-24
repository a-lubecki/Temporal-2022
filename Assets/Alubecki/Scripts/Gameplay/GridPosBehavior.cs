using System;
using DG.Tweening;
using UnityEngine;


public class GridPosBehavior : MonoBehaviour {


    public Orientation Orientation => OrientationFunctions.FindOrientation(transform.localRotation.eulerAngles.y);

    public Vector3Int GridPos => Vector3Int.RoundToInt(transform.localPosition);
    public int GridPosX => GridPos.x;
    public int GridPosY => GridPos.y;
    public int GridPosZ => GridPos.z;

    protected virtual void Start() {
        //align elem with grid
        transform.localPosition = GridPos;
    }

    public bool IsOriented(Orientation nextOrientation) {
        return Orientation == nextOrientation;
    }

    public bool IsOriented(Vector3 nextPos) {
        return IsOriented(OrientationFunctions.FindOrientation(transform.localPosition, nextPos));
    }

    public void SetOrientation(Orientation orientation) {
        transform.localRotation = Quaternion.Euler(0, (int)orientation, 0);
    }

    public bool IsOnGridPile(Vector2 horizontalPos) {
        return GridPosX == (int)horizontalPos.x && GridPosZ == (int)horizontalPos.y;
    }

    public bool IsOnGridPile(Vector3 pos) {
        return GridPosX == (int)pos.x && GridPosZ == (int)pos.z;
    }

    public bool IsOnGridPos(Vector3 pos) {
        return GridPos.Equals(Vector3Int.RoundToInt(pos));
    }

    public bool IsHorizontallyAdjacentTo(Vector3 otherPos) {
        return Vector2.Distance(new Vector2(GridPosX, GridPosZ), new Vector2((int)otherPos.x, (int)otherPos.z)) == 1;
    }

    public void SetGridPos(Vector3 pos) {
        transform.localPosition = Vector3Int.RoundToInt(pos);
    }

    public Vector3 GetNextGridPos(Orientation orientation, int height = 0, int distance = 1) {
        return GridPos + OrientationFunctions.FindNextPos(orientation) * distance + Vector3.up * height;
    }

    Tween DoOrientation(Vector3 nextPos, float durationSec) {
        return transform.DOLocalRotate(new Vector3(0, (int)OrientationFunctions.FindOrientation(transform.localPosition, nextPos), 0), durationSec);
    }

    public virtual bool TryLookAt(Vector3 nextPos, float durationSec, Action onComplete = null) {

        if (IsOriented(nextPos)) {
            //already oriented
            onComplete?.Invoke();
            return false;
        }

        var s = DOTween.Sequence();

        //look at
        s.Append(DoOrientation(nextPos, durationSec));

        //callback if necessary
        if (onComplete != null) {
            s.AppendInterval(0.01f)
                .OnComplete(() => onComplete.Invoke());
        }

        return true;
    }

    public virtual bool TryMove(Vector3 nextPos, float durationSec, Action onComplete = null, bool autoRotateBefore = false, float rotateDurationSec = 0) {

        if (IsOnGridPos(nextPos)) {
            //already on pos
            onComplete?.Invoke();
            return false;
        }

        var s = DOTween.Sequence();

        //look at if necessary
        if (autoRotateBefore && !IsOriented(nextPos)) {
            DoOrientation(nextPos, rotateDurationSec);
            s.AppendInterval(0.8f * rotateDurationSec);
        }

        //move
        s.Append(transform.DOLocalMove(Vector3Int.RoundToInt(nextPos), durationSec).SetEase(Ease.InOutQuad));

        //callback if necessary
        if (onComplete != null) {
            s.AppendInterval(0.01f)
                .OnComplete(() => onComplete.Invoke());
        }

        return true;
    }

    public virtual bool TryJump(Vector3 nextPos, float durationSec, Action onComplete = null, bool autoRotateBefore = false, float rotateDurationSec = 0) {

        if (IsOnGridPos(nextPos)) {
            //already on pos
            onComplete?.Invoke();
            return false;
        }

        var s = DOTween.Sequence();

        //look at if necessary
        if (autoRotateBefore && !IsOnGridPos(nextPos) && !IsOriented(nextPos)) {
            DoOrientation(nextPos, rotateDurationSec);
            s.AppendInterval(0.8f * rotateDurationSec);
        }

        var jumpHeight = 0.1f;
        if (GridPosY >= nextPos.y) {
            //set same height as jumping up if jumping down
            jumpHeight += (GridPosY - nextPos.y);
        }

        //jump
        s.Append(transform.DOLocalJump(Vector3Int.RoundToInt(nextPos), jumpHeight, 1, durationSec));

        //callback if necessary
        if (onComplete != null) {
            s.AppendInterval(0.01f)
                .OnComplete(() => onComplete.Invoke());
        }

        return true;
    }

    public virtual bool TryFall(int fallHeight, float durationSec, Action onComplete = null) {

        if (fallHeight < 0) {
            throw new ArgumentException("Can't have a negative fall height");
        }

        if (fallHeight == 0) {
            //already on pos
            onComplete?.Invoke();
            return false;
        }

        var s = DOTween.Sequence();

        s.Append(transform.DOLocalMove(GridPos + Vector3.down * fallHeight, durationSec).SetEase(Ease.OutBounce));

        //callback if necessary
        if (onComplete != null) {
            s.AppendInterval(0.01f)
                .OnComplete(() => onComplete.Invoke());
        }

        return true;
    }

    public virtual bool TryMoveUp(int height, float durationSec, Action onComplete = null) {

        if (height < 0) {
            throw new ArgumentException("Can't move up with a negative height");
        }

        if (height == 0) {
            //already on pos
            onComplete?.Invoke();
            return false;
        }

        var s = DOTween.Sequence();

        s.Append(transform.DOLocalMove(GridPos + Vector3.up * height, durationSec).SetEase(Ease.OutBack));

        //callback if necessary
        if (onComplete != null) {
            s.AppendInterval(0.01f)
                .OnComplete(() => onComplete.Invoke());
        }

        return true;
    }

}


public enum Orientation : int {

    North = 0,
    East = 90,
    South = 180,
    West = 270
}


class OrientationFunctions {

    public static Orientation FindOrientation(float angle) {

        //convert to orientation, if the angle is 46 degrees the result is 90 degrees:
        // 46/90 = 0.51, rounded gives 1 => 1*90 = 90
        var orientedAngle = Math.Round(angle / 90f) * 90;

        //get an angle between 0 and 359,99 => with the previous computation: only 0, 90, 180 or 270
        var sanitizedAngle = orientedAngle % 360f;

        return (Orientation)sanitizedAngle;
    }

    public static Orientation FindOrientation(Vector3 from, Vector3 to) {

        var angle = -Vector2.SignedAngle(Vector2.up, new Vector2(to.x, to.z) - new Vector2(from.x, from.z));
        return FindOrientation(angle);
    }

    public static Vector3 FindNextPos(Orientation orientation) => orientation switch {

        Orientation.North => Vector3.forward,
        Orientation.East => Vector3.right,
        Orientation.South => Vector3.back,
        Orientation.West => Vector3.left,
        _ => throw new NotImplementedException()
    };

}
