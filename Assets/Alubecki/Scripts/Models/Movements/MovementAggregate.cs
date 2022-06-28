using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Aggregate several movements and try to execute the first of the list.
/// If its not possible, try to execute the following ones.
/// </summary>
public class MovementAggregate : BaseMovement {


    List<BaseMovement.Factory> orderedFactories;
    bool needsMovementResolving;


    public override bool NeedsMovementResolving => true;


    public MovementAggregate(BaseMovement.Factory originalFactory, MovementType movementType, BaseElementBehavior owner, Vector3 nextPos, IEnumerable<BaseMovement.Factory> orderedFactories) : base(originalFactory, movementType, owner, nextPos) {

        if (orderedFactories == null || orderedFactories.Count() <= 0) {
            throw new ArgumentException("Factories params passed to MovementAggregate.Factory is null or empty");
        }

        //defensive copy
        this.orderedFactories = new List<BaseMovement.Factory>(orderedFactories);
    }

    public override IEnumerable<DisplayableMovementInfo> NewDisplayableMovementInfos() {
        return null;
    }

    protected override void ExecuteInternal(BaseElementBehavior owner, Action onComplete) {

        //execute the first possible executable movement

        //try to match horizontal pos as simple move, climb and jump could be executed for the same x/z values
        var horizontalPos = new Vector2(NextPos.x, NextPos.z);

        //create ordered movements
        foreach (var f in orderedFactories) {

            //find a target pos matching nextPos horizontally, ex : moving, climbing, jumping have a different y
            var targets = f.GetNextPossibleMovementTargets(owner);
            if (targets == null) {
                continue;
            }

            if (FindMatchingHorizontalPos(horizontalPos, targets, out var pos)) {

                f.NewMovement(owner, pos).Execute(onComplete);
                return;
            }
        }

        //fail, no movement was executed
        onComplete?.Invoke();
    }

    /// <summary>
    /// Fill a out Vector3 instead of returning it because Vector3 is a struct: impossible to know if the returned value would be a matched value
    /// </summary>
    bool FindMatchingHorizontalPos(Vector2 horizontalPos, IEnumerable<Vector3> targets, out Vector3 res) {

        foreach (var pos in targets) {

            var hPos = new Vector2(pos.x, pos.z);
            if (hPos == horizontalPos) {
                //found a matching pos horizontally
                res = pos;
                return true;
            }
        }

        res = default;
        return false;
    }


#pragma warning disable 0108
    public class Factory : BaseMovement.Factory {


        List<BaseMovement.Factory> orderedFactories;

        public override MovementType MovementType => MovementType.UNKNOWN;


        public Factory(IEnumerable<BaseMovement.Factory> orderedFactories) {

            if (orderedFactories == null || orderedFactories.Count() <= 0) {
                throw new ArgumentException("Factories params passed to MovementAggregate.Factory is null or empty");
            }

            //defensive copy
            this.orderedFactories = new List<BaseMovement.Factory>(orderedFactories);
        }

        public override IEnumerable<Vector3> GetNextPossibleMovementTargets(BaseElementBehavior owner) {

            //deduplicate all target positions with a hashset
            var res = new HashSet<Vector3>();

            //create ordered movements
            foreach (var f in orderedFactories) {

                var targets = f.GetNextPossibleMovementTargets(owner);
                if (targets != null) {
                    res.UnionWith(targets);
                }
            }

            return res;
        }

        /// <summary>
        /// Create movement with factories in additional params, it must be a IEnumerable<BaseMovement.Factory>
        /// </summary>
        public override BaseMovement NewMovement(BaseElementBehavior owner, Vector3 nextPos) {

            return new MovementAggregate(this, MovementType, owner, nextPos, orderedFactories);
        }

    }

}
