using System;
using System.Collections.Generic;


public struct MementoSnapshotBoard : IMementoSnapshot {

    readonly List<MementoSnapshotElement> elementSnapshots;

    public MementoSnapshotBoard(IEnumerable<MementoSnapshotElement> elementSnapshots) {
        //defensive copy
        this.elementSnapshots = new List<MementoSnapshotElement>(elementSnapshots);
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

}
