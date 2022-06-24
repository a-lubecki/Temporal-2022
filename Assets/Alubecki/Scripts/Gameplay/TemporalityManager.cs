using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TemporalityManager : MonoBehaviour {


    public const float DURATION_ANIM_FROM_AGE_TO_AGE_SEC = 0.2f;


    public void InitZones() {

        DeleteZones();

        var temporalAbilities = Game.Instance.boardBehavior.GetElements().Distinct()
            .Where(e => e.TryGetComponent<TemporalAbilityBehavior>(out _))
            .Select(e => e.GetComponent<TemporalAbilityBehavior>());

        foreach (var ability in temporalAbilities) {

            var zone = Game.Instance.poolTemporalZone.Spawn(transform);
            zone.GetComponent<TemporalZoneBehavior>().SetOwner(ability);
        }
    }

    public void DeleteZones() {
        Game.Instance.poolTemporalZone.DespawnAll();
    }

    public IEnumerable<TemporalZoneBehavior> GetZones() {
        return GetComponentsInChildren<TemporalZoneBehavior>().Where(z => z.IsTemporalityActive);
    }


    public void ComputeTemporalityAndGravity() {

        ClearParadoxes();

        var aboutToFallElements = Game.Instance.boardBehavior.GetAboutToFallElements();

        var nbLoops = 0;

        var agesBeforeResolve = new Dictionary<BaseElementBehavior, int>();
        var positionsBeforeResolve = new Dictionary<BaseElementBehavior, Vector3>();
        var paradoxPositionElements = new List<BaseElementBehavior>();

        do {
            ResolveGravity(aboutToFallElements, positionsBeforeResolve, paradoxPositionElements);

            agesBeforeResolve.Clear();
            ResolveTemporality(agesBeforeResolve, positionsBeforeResolve, paradoxPositionElements);

            ResolveParadoxes(agesBeforeResolve, positionsBeforeResolve, paradoxPositionElements);

            nbLoops++;

            if (nbLoops >= 3) {
                break;
            }

            aboutToFallElements = Game.Instance.boardBehavior.GetAboutToFallElements();

        } while (aboutToFallElements.Count() > 0);
    }

    void ClearParadoxes() {

        //clear paradox states
        var agedElements = Game.Instance.boardBehavior.GetElements().Where(e => e.TryGetComponent<AgeParadoxBehavior>(out _));

        foreach (var elem in agedElements) {
            elem.GetComponent<AgeParadoxBehavior>().ClearParadoxState();
        }
    }

    void ResolveGravity(IEnumerable<BaseElementBehavior> aboutToFallElements, Dictionary<BaseElementBehavior, Vector3> positionsBeforeResolve, List<BaseElementBehavior> paradoxPositionElements) {

        foreach (var elem in aboutToFallElements) {
            ResolveGravityForElement(elem, positionsBeforeResolve, paradoxPositionElements);
        }
    }

    void ResolveGravityForElement(BaseElementBehavior elem, Dictionary<BaseElementBehavior, Vector3> positionsBeforeResolve, List<BaseElementBehavior> paradoxPositionElements) {

        var fallHeight = Game.Instance.boardBehavior.GetFallHeight(elem);
        if (fallHeight <= 0) {
            //no changes
            return;
        }

        var elemsToFall = Game.Instance.boardBehavior.GetSortedElementsAbove(elem)
            .Prepend(elem);

        foreach (var e in elemsToFall) {

            if (paradoxPositionElements != null && paradoxPositionElements.Contains(e)) {
                //in paradox state, avoid an infinite loop
                continue;
            }

            if (!positionsBeforeResolve.ContainsKey(e)) {
                positionsBeforeResolve.Add(e, e.GridPos);
            }

            //fall
            e.SetGridPos(e.GridPos + Vector3.down * fallHeight);
        }
    }

    public void ResolveTemporality(Dictionary<BaseElementBehavior, int> agesBeforeResolve, Dictionary<BaseElementBehavior, Vector3> positionsBeforeResolve, List<BaseElementBehavior> paradoxPositionElements) {

        var zones = GetZones();
        var agedElements = Game.Instance.boardBehavior.GetElements().Where(e => e.TryGetComponent<AgeBehavior>(out _));
        /*
                var agedElemsByAges = agedElements.Select(e => new KeyValuePair<BaseElementBehavior, int>(e, e.GetComponent<AgeBehavior>().CurrentAge));
                ResolveParadoxesInternal(agedElemsByAges.ToDictionary(e => e.Key, e => e.Value));
        */
        foreach (var elem in agedElements) {

            if (IsInParadoxState(elem)) {
                //avoid changing age if already in paradox
                continue;
            }

            var previousAge = elem.GetComponent<AgeBehavior>().CurrentAge;

            if (ResolveTemporalityForElement(elem, zones, positionsBeforeResolve, paradoxPositionElements)) {

                //only retain age if temporality was resolved (this dictionary is used to resolve paradoxes in case age changed)
                if (!agesBeforeResolve.ContainsKey(elem)) {
                    agesBeforeResolve.Add(elem, previousAge);
                }
            }
        }
    }

    bool IsInParadoxState(BaseElementBehavior elem) {
        return elem.GetComponent<AgeParadoxBehavior>()?.IsInParadoxState ?? false;
    }

    public void ResolveParadoxes(Dictionary<BaseElementBehavior, int> agesBeforeResolve, Dictionary<BaseElementBehavior, Vector3> positionsBeforeResolve, List<BaseElementBehavior> paradoxPositionElements) {

        var zones = GetZones();

        //resolve paradoxes
        foreach (var e in agesBeforeResolve) {

            var elem = e.Key;
            var previousAge = e.Value;

            elem.TryGetComponent<AgeParadoxBehavior>(out var ageParadoxBehavior);

            if (ageParadoxBehavior != null && previousAge != ageParadoxBehavior.CurrentAge) {

                //reset the previous age because it has been replaced by the current age after the animation
                //previous age is needed by the algorithm in AgeParadoxBehavior
                ageParadoxBehavior.OverridePreviousAge(previousAge);

                //if nothing changed after the next resolve, the elem is in paradox state, it won't be animated as it's in the previous age
                if (!ResolveTemporalityForElement(elem, zones, positionsBeforeResolve, paradoxPositionElements)) {

                    //resolve gravity for the pile and mark the objects as paradox
                    var elementsAbove = Game.Instance.boardBehavior.GetSortedElementsAbove(elem);
                    ResolveGravity(elementsAbove, positionsBeforeResolve, paradoxPositionElements);

                    foreach (var eAbove in elementsAbove) {

                        if (paradoxPositionElements != null && !paradoxPositionElements.Contains(eAbove)) {
                            paradoxPositionElements.Add(eAbove);
                        }
                    }
                }
            }
        }
    }

    bool ResolveTemporalityForElement(BaseElementBehavior elem, IEnumerable<TemporalZoneBehavior> zones, Dictionary<BaseElementBehavior, Vector3> positionsBeforeResolve, List<BaseElementBehavior> paradoxPositionElements) {

        var totalAgeShift = 0;

        //get influence of all zones on elem
        foreach (var zone in zones) {

            if (zone.IsOwner(elem)) {
                //the character is not impacted by its own zone
                continue;
            }

            if (zone.ContainsElement(elem)) {
                totalAgeShift += zone.AgeShiftYears;
            }
        }

        var ageBehavior = elem.GetComponent<AgeBehavior>();

        //change age
        var changed = ageBehavior.SetCurrentAge(ageBehavior.RealAge + totalAgeShift, false);
        if (changed) {
            PushElementsAboveIfNecessary(elem, ageBehavior, positionsBeforeResolve, paradoxPositionElements);
            return true;
        }

        return false;
    }

    void PushElementsAboveIfNecessary(BaseElementBehavior elem, AgeBehavior ageBehavior, Dictionary<BaseElementBehavior, Vector3> positionsBeforeResolve, List<BaseElementBehavior> paradoxPositionElements) {

        var previousColliderHeight = elem.GetColliderHeightForAge(ageBehavior.PreviousAge);
        var nextColliderHeight = elem.GetColliderHeightForAge(ageBehavior.CurrentAge);

        var diff = nextColliderHeight - previousColliderHeight;
        if (diff <= 0) {
            //no need to push elements up
            return;
        }

        var board = Game.Instance.boardBehavior;

        //register all elements on elem pos and above to push them after
        var elementsToPush = new List<BaseElementBehavior>();
        var currentPos = elem.GridPos + Vector3.up * previousColliderHeight;
        IEnumerable<BaseElementBehavior> elemsOnPos;

        do {
            elemsOnPos = board.GetElemsOnPos(currentPos).Where(e => e != elem && e.CanFall);
            elementsToPush.AddRange(elemsOnPos);

            currentPos.y++;

        } while (elemsOnPos.Count() > 0);

        //push all elements up at the same time with the same diff height
        foreach (var e in elementsToPush) {

            if (paradoxPositionElements != null && paradoxPositionElements.Contains(e)) {
                //avoid an infinite loop
                continue;
            }

            if (positionsBeforeResolve != null && !positionsBeforeResolve.ContainsKey(e)) {
                positionsBeforeResolve.Add(e, e.GridPos);
            }

            //move up
            e.SetGridPos(e.GridPos + Vector3.up * diff);
        }
    }

}
