using System;
using UnityEngine;


public class TemporalZoneBehavior : MonoBehaviour {


    public const float ZONE_EDGE_SIZE = 3;


    GameObject goMesh;
    MeshRenderer[] meshRenderersZone;
    [SerializeField] TemporalAbilityBehavior ownerElem;

    [SerializeField] int ageShiftYears = 0;

    public int AgeShiftYears => ageShiftYears;
    public bool IsTemporalityActive => (goMesh.activeInHierarchy && ownerElem != null);


    void Awake() {
        goMesh = transform.GetChild(0).gameObject;
        meshRenderersZone = GetComponentsInChildren<MeshRenderer>();
    }

    void Update() {

        if (IsTemporalityActive) {

            transform.position = ownerElem.transform.position;

            if (!goMesh.activeSelf) {
                goMesh.SetActive(true);
            }

        } else {

            goMesh.SetActive(false);
        }
    }

    public void SetOwner(TemporalAbilityBehavior ownerElem) {

        this.ownerElem = ownerElem;

        if (ownerElem != null) {
            UpdateZone(ownerElem.TemporalAbility);
        }
    }

    void UpdateZone(DataTemporalAbility data) {

        ageShiftYears = data.AgeShiftYears;

        foreach (var r in meshRenderersZone) {
            r.material = data.ZoneMaterial;
        }
    }

    public bool IsOwner(BaseElementBehavior elem) {

        if (elem == null) {
            throw new ArgumentException();
        }

        return elem.gameObject == ownerElem.gameObject;
    }


    static Bounds boundZone = new Bounds(Vector3.zero, Vector3.one * (ZONE_EDGE_SIZE - 0.02f));
    static Bounds boundHeight = new Bounds();
    static Vector3 halfElemSize = new Vector3(0.01f, 0, 0.01f);
    static Vector3 shiftUp = 0.5f * Vector3.up;

    /// <summary>
    /// Intersect 2 bounding boxes to know if the zone "touches" the element.
    /// For the element, it uses a bounding box instead of a ray because ray intersection is buggy.
    /// //It uses static Bound and Vector obejcts for optimizations.
    /// </summary>
    public bool ContainsElement(BaseElementBehavior elem) {

        boundZone.center = transform.position + shiftUp;

        var colliderHeight = elem.ColliderHeight;
        var shiftY = (colliderHeight <= 1) ? shiftUp : shiftUp * colliderHeight;
        boundHeight.center = elem.transform.position + shiftY;

        halfElemSize.y = (colliderHeight <= 1) ? 0.01f : (0.5f * colliderHeight - 0.01f);
        boundHeight.extents = halfElemSize;

        return boundZone.Intersects(boundHeight);
    }

}
