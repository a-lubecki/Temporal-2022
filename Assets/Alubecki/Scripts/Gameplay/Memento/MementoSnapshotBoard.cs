using System;
using System.Collections.Generic;


public struct MementoSnapshotBoard : IMementoSnapshot {


    readonly HashSet<MementoSnapshotElement> elementSnapshots;

    public readonly int aiTeamEnemyTurnCount;
    public readonly int aiTeamEnemyLastTurnWithMovement;
    public readonly int aiTeamNeutralTurnCount;
    public readonly int aiTeamNeutralLastTurnWithMovement;


    public MementoSnapshotBoard(IEnumerable<MementoSnapshotElement> elementSnapshots, int aiTeamEnemyTurnCount, int aiTeamEnemyLastTurnWithMovement, int aiTeamNeutralTurnCount, int aiTeamNeutralLastTurnWithMovement) {

        //defensive copy
        this.elementSnapshots = new HashSet<MementoSnapshotElement>(elementSnapshots);

        this.aiTeamEnemyTurnCount = aiTeamEnemyTurnCount;
        this.aiTeamEnemyLastTurnWithMovement = aiTeamEnemyLastTurnWithMovement;
        this.aiTeamNeutralTurnCount = aiTeamNeutralTurnCount;
        this.aiTeamNeutralLastTurnWithMovement = aiTeamNeutralLastTurnWithMovement;
    }

    public IEnumerable<MementoSnapshotElement> GetElementsSnapshot() {
        //defensive copy
        return new List<MementoSnapshotElement>(elementSnapshots);
    }

    public void ProcessElements(IEnumerable<BaseElementBehavior> elements, Action<BaseElementBehavior, MementoSnapshotElement> processMatch) {

        //create a new list to remove elements for optimization
        var snapshots = new List<MementoSnapshotElement>(elementSnapshots);

        //find corresponding snapshots for each elem
        foreach (var elem in elements) {

            var id = elem.GetInstanceID();

            for (int iss = 0; iss < snapshots.Count; iss++) {

                var s = (MementoSnapshotElement)snapshots[iss];

                if (id == s.instanceId) {

                    //found corresponding snapshot for elem
                    processMatch(elem, s);

                    //remove snapshot from the list for optimization as we will never find it again for another elem
                    snapshots.RemoveAt(iss);
                    break;
                }
            }
        }
    }

    public override bool Equals(object obj) {

        if (obj == null || !(obj is MementoSnapshotBoard)) {
            return false;
        }

        return elementSnapshots.SetEquals(((MementoSnapshotBoard)obj).elementSnapshots);
    }

    public override int GetHashCode() {
        throw new NotImplementedException("Not implemented yet because not used as key in dictionary");
    }

}
