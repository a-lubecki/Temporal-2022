using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[DisallowMultipleComponent]
public class AgeBehavior : MonoBehaviour {


    //steps between ages must be 20 years, for example : [10, 30, 50] or [0, 20, 40]
    public const float AGE_STEPS_IN_YEARS = 20;
    public const float HALF_AGE_STEPS_IN_YEARS = AGE_STEPS_IN_YEARS / 2f;

    public const string GO_NAME_AGED_MESHES = "AgedMeshes";
    const float ANIM_PERIOD_SEC = 0.02f;


    [SerializeField] int realAge;
    [SerializeField] int currentAge;
    [SerializeField] int maxAge = -1;

    readonly Dictionary<AgeBounds, GameObject> meshesByAgeBounds = new Dictionary<AgeBounds, GameObject>();
    Coroutine coroutineAnimateAgeChanges;

    public int RealAge => realAge;
    public int CurrentAge => currentAge;
    public int PreviousAge { get; private set; }
    public bool DidAgeChange => currentAge != PreviousAge;
    public bool IsAnimatingAgeChange => coroutineAnimateAgeChanges != null;
    public bool HasReachMaxAge => currentAge >= maxAge;
    public virtual string DisplayableText => "<b>Age:</b> " + currentAge;
    public bool AreMeshesEnabled { get; private set; }


    protected virtual void Awake() {

        //init aged meshes dictionary
        var meshesByAge = GetMeshesByAge(GO_NAME_AGED_MESHES);

        //shift the bounds negatively to have symetry when changing meshes between getting older and getting younger
        //get the half step between max age and min age, cap to HALF_AGE_STEPS_IN_YEARS to avoid unwanted big steps
        AgeBounds.ConvertAgesToAgeBounds(
            meshesByAge,
            meshesByAgeBounds,
            (minAge, maxAge) => - (int)Mathf.Min(0.5f * (maxAge - minAge), HALF_AGE_STEPS_IN_YEARS)
        );
    }

    protected virtual void OnEnable() {

        currentAge = realAge;
        PreviousAge = currentAge;
        UpdateMeshesWithCurrentAge();
    }

    protected virtual void Update() {
        //reset previous age to reset DidAgeChange (used for updating other classes depending on AgeBehavior)
        PreviousAge = currentAge;
    }

    protected Dictionary<int, GameObject> GetMeshesByAge(string meshesParentName) {

        var trMeshes = transform.Find(meshesParentName);
        if (trMeshes == null) {
            throw new NotSupportedException("An aged object must have an child object named " + GO_NAME_AGED_MESHES);
        }

        var res = new Dictionary<int, GameObject>();

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

            if (res.ContainsKey(age)) {
                throw new InvalidOperationException("Mesh with the same age detected : " + age);
            }

            res.Add(age, go);
        }

        return res;
    }

    public void InitAges(int initialAge, int maxAge) {

        this.realAge = initialAge;
        this.maxAge = maxAge;

        currentAge = realAge;
        PreviousAge = currentAge;
        UpdateMeshesWithCurrentAge();
    }

    public void InitCurrentAge(int currentAge) {

        this.currentAge = currentAge;
        PreviousAge = currentAge;
        UpdateMeshesWithCurrentAge();
    }

    protected bool AreAgesInSameAgeBounds(int age1, int age2) {

        foreach (var e in meshesByAgeBounds) {

            if (e.Key.IsInBounds(age1) && e.Key.IsInBounds(age2)) {
                //same age bounds
                return true;
            }
        }

        return false;
    }

    public GameObject FindMeshForAge(int age) {
        return AgeBounds.FindDataForAge(meshesByAgeBounds, age);
    }

    public virtual bool SetCurrentAge(int age, bool animated = false, float durationSec = 0, Action onComplete = null) {

        if (currentAge == age) {
            //already the same age
            return false;
        }

        PreviousAge = currentAge;
        currentAge = age;

        //stop previous animation even if it's not animated now
        if (coroutineAnimateAgeChanges != null) {
            StopCoroutine(coroutineAnimateAgeChanges);
        }

        if (!animated || durationSec <= ANIM_PERIOD_SEC) {
            UpdateMeshesWithCurrentAge();
            return true;
        }

        //animate changes
        coroutineAnimateAgeChanges = StartCoroutine(AnimateMeshesVisibility(PreviousAge, currentAge, durationSec, onComplete));

        return true;
    }

    /// <summary>
    /// Change the previous age for algorithms purpose during the current frame if necessary
    /// </summary>
    public void OverridePreviousAge(int age) {
        PreviousAge = age;
    }

    IEnumerator AnimateMeshesVisibility(int previousAge, int currentAge, float durationSec, Action onComplete = null) {

        var previousTimeSec = 0f;
        var elapsedSec = 0f;

        UpdateMeshesWithAge(previousAge);

        while (elapsedSec < durationSec) {

            previousTimeSec = Time.timeSinceLevelLoad;

            yield return new WaitForSeconds(ANIM_PERIOD_SEC);

            elapsedSec += Time.timeSinceLevelLoad - previousTimeSec;

            //go forward or backward with step of 1 year
            var percentage = elapsedSec / durationSec;
            if (percentage > 1) {
                percentage = 1;
            }

            var age = (int)(previousAge + percentage * (currentAge - previousAge));
            UpdateMeshesWithAge(age);
        }

        //ensure the final age is correct
        UpdateMeshesWithAge(currentAge);

        coroutineAnimateAgeChanges = null;

        onComplete?.Invoke();
    }

    public void SetMeshesEnabled(bool enabled) {

        AreMeshesEnabled = enabled;
        UpdateMeshesWithCurrentAge();
    }

    public void UpdateMeshesWithCurrentAge() {
        UpdateMeshesWithAge(currentAge);
    }

    public void UpdateMeshesWithAge(int age) {

        //only activate the mesh for current age
        foreach (var e in meshesByAgeBounds) {
            e.Value.SetActive(AreMeshesEnabled && e.Key.IsInBounds(age));
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


    /// <summary>
    /// Take a dictionary of data by age, sort it by age and fill a dictionary of data by age bounds (2 values: min an max ages).
    /// The Func functionShiftMaxBound allow the caller to shift all bounds in positive or negative if needed:
    ///     - Func< int, int, int >
    ///     - Func< returned shift, min age of the current bound, max age of the current bound>
    /// </summary>
    public static void ConvertAgesToAgeBounds<T>(Dictionary<int, T> dataByAge, Dictionary<AgeBounds, T> dataByAgeBoundsToFill, Func<int, int, int> functionShiftMaxBound = null) {

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
            var data = dataByAge[minAge];

            //min bound is previous max bound
            if (i > 0) {
                minBound = maxBound;
            }

            maxBound = maxAge;
            if (functionShiftMaxBound != null) {
                maxBound += functionShiftMaxBound(minAge, maxAge);
            }

            dataByAgeBoundsToFill.Add(new AgeBounds(minBound, maxBound), data);
        }

        dataByAgeBoundsToFill.Add(new AgeBounds(maxBound, int.MaxValue), dataByAge.Last().Value);
    }

    public static T FindDataForAge<T>(Dictionary<AgeBounds, T> dataByAgeBounds, int age) {
        return dataByAgeBounds.FirstOrDefault(e => e.Key.IsInBounds(age)).Value;
    }

}
