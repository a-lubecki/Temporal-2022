using System;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(BaseElementBehavior))]
public class AgeParadoxBehavior : AgeBehavior {


    public const string GO_NAME_PARADOX_MESHES = "ParadoxMeshes";


    BaseElementBehavior elem;
    Dictionary<int, GameObject> meshesByParadoxAge;

    public bool IsInParadoxState { get; private set; }
    public int ParadoxAge { get; private set; }

    public override string DisplayableText => base.DisplayableText + GetParadoxAgeText();

    string GetParadoxAgeText() {

        if (!IsInParadoxState) {
            return "";
        }

        return "\n<size=80%>Is in a <u>paradox</u> state because a temporal zone prevents it from turning " + ParadoxAge + ".</size>";
    }

    protected override void Awake() {
        base.Awake();

        elem = GetComponent<BaseElementBehavior>();
        meshesByParadoxAge = GetMeshesByAge(GO_NAME_PARADOX_MESHES);
    }

    void Start() {

        HideParadoxMeshes();
    }

    public void ClearParadoxState() {

        HideParadoxMeshes();

        IsInParadoxState = false;
        ParadoxAge = 0;
    }

    public override bool SetCurrentAge(int age, bool animated = false, float durationSec = 0, Action onComplete = null) {
        return OverrideCurrentAge(true, age, animated, durationSec, onComplete);
    }

    public bool OverrideCurrentAge(bool mustResolveParadoxes, int age, bool animated = false, float durationSec = 0, Action onComplete = null) {

        if (!mustResolveParadoxes) {
            //simple resolve
            return base.SetCurrentAge(age, animated, durationSec, onComplete);
        }

        if (IsInParadoxState && (age == ParadoxAge || age == CurrentAge)) {
            return false;
        }

        HideParadoxMeshes();

        var previousAge = PreviousAge;
        var hasChanged = base.SetCurrentAge(age, animated, durationSec, onComplete);

        if (previousAge == CurrentAge && PreviousAge != CurrentAge) {

            //the object stay with the same age but displays the paradox of the previous age
            if (!ShowParadoxMesh(PreviousAge)) {
                //invert ages
                ShowParadoxMesh(CurrentAge);
                base.SetCurrentAge(PreviousAge);
            }
        }

        return hasChanged;
    }

    public bool ShowParadoxMesh(int age) {

        //chack if age isin the same age bounds to show the right paradox mesh
        foreach (var e in meshesByParadoxAge) {

            if (AreAgesInSameAgeBounds(e.Key, age)) {
                //found mesh
                e.Value.SetActive(true);
                IsInParadoxState = true;
                ParadoxAge = age;
                return true;
            }
        }

        return false;
    }

    public void HideParadoxMeshes() {

        foreach (var e in meshesByParadoxAge) {
            e.Value.SetActive(false);
        }

        IsInParadoxState = false;
    }

}
