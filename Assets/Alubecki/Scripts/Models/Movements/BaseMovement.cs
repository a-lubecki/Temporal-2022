using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class BaseMovement {


    Factory originalFactory;

    //use weak reference in case the object needs to be garbase collected
    WeakReference<BaseElementBehavior> ownerRef;

    public BaseElementBehavior Owner {
        get {
            if (ownerRef.TryGetTarget(out var elem) && elem.isActiveAndEnabled) {
                return elem;
            }
            return null;
        }
    }

    public MovementType MovementType { get; private set; }
    public Vector3 NextPos { get; private set; }
    public virtual string DisplayableName => "(unknown)";
    public abstract bool NeedsMovementResolving { get; }


    public BaseMovement(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos) {

        this.originalFactory = originalFactory;
        MovementType = movementType;
        ownerRef = new WeakReference<BaseElementBehavior>(owner);
        NextPos = nextPos;
    }

    public abstract IEnumerable<DisplayableMovementInfo> NewDisplayableMovementInfos();

    public bool CanExecute() {

        var owner = Owner;
        if (owner == null) {
            return false;
        }

        var possibleMovements = originalFactory.GetNextPossibleMovementTargets(owner);
        if (!possibleMovements.Contains(NextPos)) {
            //something changed, the target pos is not available any more
            return false;
        }

        return true;
    }

    public void Execute(Action onComplete) {

        var owner = Owner;
        if (owner != null) {
            ExecuteInternal(owner, onComplete);
        } else {
            onComplete?.Invoke();
        }
    }

    protected abstract void ExecuteInternal(BaseElementBehavior owner, Action onComplete);


    public abstract class Factory {

        public abstract MovementType MovementType { get; }

        public abstract IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner);

        public abstract BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos);

    }

}

public enum MovementType {
    UNKNOWN,
    SIMPLE_MOVE,
    ORIENTATION,
    ACTION_ONE_TIME,
    ACTION_CHANGE_STATE,
}

public class DisplayableMovementInfo {

    public MovementDisplay Display { get; private set; }
    public Vector3 Pos { get; private set; }
    public Orientation Orientation { get; private set; }
    public BaseMovement Movement { get; private set; }

    public DisplayableMovementInfo(MovementDisplay display, Vector3 pos, Orientation orientation, BaseMovement movement) {
        Display = display;
        Pos = pos;
        Orientation = orientation;
        Movement = movement;
    }

}

public enum MovementDisplay {
    SQUARE,
    BLOCK,
    BUTTON
}
