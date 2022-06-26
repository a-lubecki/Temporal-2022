using UnityEngine;


public struct MementoSnapshotElement : IMementoSnapshot {


    public readonly int instanceId;
    public readonly Vector3 localPos;
    public readonly Quaternion localRot;
    public readonly bool isDead;
    public readonly int age;
    public readonly bool isInParadoxState;
    public readonly int ageInParadox;


    public MementoSnapshotElement(int instanceId, Vector3 localPos, Quaternion localRot, bool isDead, int age, bool isInParadoxState, int ageInParadox) {

        this.instanceId = instanceId;
        this.localPos = localPos;
        this.localRot = localRot;
        this.isDead = isDead;
        this.age = age;
        this.isInParadoxState = isInParadoxState;
        this.ageInParadox = ageInParadox;
    }

}