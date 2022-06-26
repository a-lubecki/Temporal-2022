using UnityEngine;


public class GroundBehavior : BaseElementBehavior {


    public override DisplayableCharacteristics DisplayableCharacteristics => null;


    protected virtual void OnDrawGizmos() {

        if (transform.GetComponentsInChildren<MeshRenderer>().Length <= 0) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + 0.5f * Vector3.up, Vector3.one);
        }
    }

}
