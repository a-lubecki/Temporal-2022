using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AgeBehavior : MonoBehaviour {


    public const string GO_NAME_MESHES = "AgedMeshes";
    const float ANIM_PERIOD_SEC = 0.02f;


    [SerializeField] int realAge;
    [SerializeField] int currentAge;
    [SerializeField] int maxAge = -1;

    readonly Dictionary<AgeBounds, GameObject> meshesByAgeBounds = new Dictionary<AgeBounds, GameObject>();
    Coroutine coroutineAnimateAgeChanges;

    public int CurrentAge => currentAge;
    public int PreviousAge { get; private set; }
    public bool DidAgeChange => currentAge != PreviousAge;
    public bool IsAnimatingAgeChange => coroutineAnimateAgeChanges != null;
    public bool HasReachMaxAge => currentAge >= maxAge;
    public string DisplayableText => "<b>Age:</b> " + currentAge;


    void Awake() {

        InitAgedMeshes();
    }

    void OnEnable() {

        currentAge = realAge;
        PreviousAge = currentAge;
        UpdateMeshesWithCurrentAge();
    }

    void Update() {
        //reset previous age to reset DidAgeChange (used for updating other classes depending on AgeBehavior)
        PreviousAge = currentAge;
    }

    void InitAgedMeshes() {

        var trMeshes = transform.Find(GO_NAME_MESHES);
        if (trMeshes == null) {
            throw new NotSupportedException("An aged object must have an child object named " + GO_NAME_MESHES);
        }

        var meshesByAge = new Dictionary<int, GameObject>();

        //find all behaviors with this pattern : "name#XX" where XX is the age of the element
        foreach (Transform t in trMeshes) {

            var go = t.gameObject;
            var name = go.name;

            //find age of the current sub object
            var anchorIndex = go.name.LastIndexOf('#');
            if (anchorIndex <= 0) {
                throw new NotSupportedException("Mesh age not found : " + name);
            }

            var age = 0;
            int.TryParse(name.Substring(anchorIndex + 1), out age);

            if (meshesByAge.ContainsKey(age)) {
                throw new InvalidOperationException("Mesh with the same age detected : " + age);
            }

            meshesByAge.Add(age, go);
        }

        AgeBounds.ConvertAgesToAgeBounds(meshesByAge, meshesByAgeBounds);
    }

    public void InitAges(int initialAge, int maxAge) {

        this.realAge = initialAge;
        this.maxAge = maxAge;

        currentAge = realAge;
        PreviousAge = currentAge;
        UpdateMeshesWithCurrentAge();
    }

    public GameObject FindMeshForAge(int age) {
        return AgeBounds.FindDataForAge(meshesByAgeBounds, age);
    }

    public bool SetAgeShift(int ageShift, bool animated = false, float durationSec = 0) {
        return SetCurrentAge(realAge + ageShift, animated, durationSec);
    }

    public bool SetCurrentAge(int age, bool animated = false, float durationSec = 0) {

        if (currentAge == age) {
            //already the same age
            return false;
        }

        PreviousAge = currentAge;
        currentAge = age;

        if (!animated || durationSec <= ANIM_PERIOD_SEC) {
            UpdateMeshesWithCurrentAge();
            return true;
        }

        //animate changes
        if (coroutineAnimateAgeChanges != null) {
            StopCoroutine(coroutineAnimateAgeChanges);
        }

        coroutineAnimateAgeChanges = StartCoroutine(AnimateMeshesVisibility(PreviousAge, currentAge, durationSec));

        return true;
    }

    IEnumerator AnimateMeshesVisibility(int previousAge, int currentAge, float durationSec) {

        var previousTimeSec = 0f;
        var elapsedSec = 0f;

        UpdateMeshesWithAge(previousAge);

        while (elapsedSec < durationSec) {

            previousTimeSec = Time.timeSinceLevelLoad;

            yield return new WaitForSeconds(ANIM_PERIOD_SEC);

            elapsedSec += Time.timeSinceLevelLoad - previousTimeSec;

            //go forward or backward with step of 1 year
            var percentage = elapsedSec / durationSec;
            if (percentage > 0) {
                percentage = 1;
            }

            var age = (int)(previousAge + percentage * (currentAge - previousAge));
            UpdateMeshesWithAge(age);
        }

        //ensure the final age is correct
        UpdateMeshesWithAge(currentAge);

        coroutineAnimateAgeChanges = null;
    }

    public void UpdateMeshesWithCurrentAge() {
        UpdateMeshesWithAge(currentAge);
    }

    public void UpdateMeshesWithAge(int age) {

        //only activate the mesh for current age
        foreach (var e in meshesByAgeBounds) {
            e.Value.SetActive(e.Key.IsInBounds(age));
        }
    }

}


public struct AgeBounds {

    public readonly int minAgeIncluded;
    public readonly int maxAgeExcluded;

    public AgeBounds(int age0, int age1) {

        if (age0 == age1) {
            throw new ArgumentException();
        }

        if (age0 < age1) {
            minAgeIncluded = age0;
            maxAgeExcluded = age1;
        } else {
            minAgeIncluded = age1;
            maxAgeExcluded = age0;
        }
    }

    public bool IsInBounds(int age) {
        return minAgeIncluded <= age && age < maxAgeExcluded;
    }


    //steps between ages must be 20 years, for example : [10, 30, 50] or [0, 20, 40]
    public const float AGE_STEPS_IN_YEARS = 20;
    public const float HALF_AGE_STEPS_IN_YEARS = AGE_STEPS_IN_YEARS / 2f;

    public static void ConvertAgesToAgeBounds<T>(Dictionary<int, T> dataByAge, Dictionary<AgeBounds, T> dataByAgeBoundsToFill) {

        dataByAgeBoundsToFill.Clear();

        //fill meshes by bounds :
        var ages = new List<int>(dataByAge.Keys);
        var agesCount = ages.Count;
        if (agesCount <= 0) {
            throw new NotSupportedException("Can't get age");
        }

        ages.Sort();

        int minBound = int.MinValue;
        int maxBound = minBound;

        for (int i = 0; i < agesCount - 1; i++) {

            var minAge = ages[i];
            var maxAge = ages[i + 1];
            var mesh = dataByAge[minAge];

            //min bound is previous max bound
            if (i > 0) {
                minBound = maxBound;
            }

            //get the half step between max age and min age, cap to HALF_AGE_STEPS_IN_YEARS to avoid unwanted big steps
            maxBound = maxAge - (int)Mathf.Min(0.5f * (maxAge - minAge), HALF_AGE_STEPS_IN_YEARS);

            dataByAgeBoundsToFill.Add(new AgeBounds(minBound, maxBound), mesh);
        }

        dataByAgeBoundsToFill.Add(new AgeBounds(maxBound, int.MaxValue), dataByAge.Last().Value);
    }

    public static T FindDataForAge<T>(Dictionary<AgeBounds, T> dataByAgeBounds, int age) {
        return dataByAgeBounds.FirstOrDefault(e => e.Key.IsInBounds(age)).Value;
    }

}