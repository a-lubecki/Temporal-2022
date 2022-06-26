using System;
using UnityEngine;


public struct MementoSnapshotElement : IMementoSnapshot {


    public readonly int instanceId;
    public readonly bool hasInvisibleMeshes;
    public readonly Vector3 localPos;
    public readonly Quaternion localRot;
    public readonly bool isDead;
    public readonly int age;
    public readonly bool isInParadoxState;
    public readonly int ageInParadox;


    public MementoSnapshotElement(int instanceId, bool hasInvisibleMeshes, Vector3 localPos, Quaternion localRot, bool isDead, int age, bool isInParadoxState, int ageInParadox) {

        this.instanceId = instanceId;
        this.hasInvisibleMeshes = hasInvisibleMeshes;
        this.localPos = localPos;
        this.localRot = localRot;
        this.isDead = isDead;
        this.age = age;
        this.isInParadoxState = isInParadoxState;
        this.ageInParadox = ageInParadox;
    }

    public override bool Equals(object obj) {

        if (obj == null || !(obj is MementoSnapshotElement)) {
            return false;
        }

        var o = (MementoSnapshotElement)obj;

        return instanceId == o.instanceId &&
            hasInvisibleMeshes == o.hasInvisibleMeshes &&
            localPos == o.localPos &&
            localRot == o.localRot &&
            isDead == o.isDead &&
            age == o.age &&
            isInParadoxState == o.isInParadoxState &&
            ageInParadox == o.ageInParadox;
    }

    public override int GetHashCode() {
        return instanceId.GetHashCode();
    }

}
