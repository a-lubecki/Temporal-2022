using System;
using UnityEngine;


/// <summary>
/// By default all the elements have a height of 1 block.
/// Sometimes, the mesh/collider height change with the age.
/// Add this component to a mesh of an element to ensure its current height is taken into account in the computations.
/// </summary>
public class OverridenHeightBehavior : MonoBehaviour {


    [field: SerializeField] public int ColliderHeight { get; protected set; }

    void Awake() {
        if (ColliderHeight < 0) {
            throw new InvalidOperationException("Can't have negative collider height");
        }
    }

}
