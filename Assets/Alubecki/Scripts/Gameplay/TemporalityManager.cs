using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TemporalityManager : MonoBehaviour {


    public const float DURATION_ANIM_FROM_AGE_TO_AGE_SEC = 0.2f;


    Dictionary<BaseElementBehavior, int> agesBeforeResolve = new Dictionary<BaseElementBehavior, int>();


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

    public IEnumerator ResolveTemporality() {

        var isAnimatingZones = false;
        var zones = GetZones();
        var agedElements = Game.Instance.boardBehavior.GetElements().Where(e => e.TryGetComponent<AgeBehavior>(out _));

        agesBeforeResolve.Clear();

        foreach (var elem in agedElements) {

            var previousAge = elem.GetComponent<AgeBehavior>().PreviousAge;

            if (ResolveTemporalityForElement(elem, zones, true)) {

                isAnimatingZones = true;

                //only retain age if temporality was resolved (this dictionary is used t resolve paradoxes in case age changed)
                agesBeforeResolve.Add(elem, previousAge);
            }
        }

        if (isAnimatingZones) {
            yield return new WaitForSeconds(DURATION_ANIM_FROM_AGE_TO_AGE_SEC + 0.1f);
        }
    }

    public void ResolveParadoxes() {

        var zones = GetZones();

        //resolve paradoxes
        foreach (var e in agesBeforeResolve) {

            var elem = e.Key;
            var previousAge = e.Value;

            var ageBehavior = elem.GetComponent<AgeBehavior>();

            if (previousAge != ageBehavior.CurrentAge) {

                //reset the previous age because it has been replaced by the current age after the animation
                //previous age is needed by the algorithm in AgeParadoxBehavior
                ageBehavior.OverridePreviousAge(previousAge);

                //if nothing changed after the next resolve, the elem is in paradox state, it won't be animated as it's in the previous age
                ResolveTemporalityForElement(elem, zones, false);
            }
        }
    }

    bool ResolveTemporalityForElement(BaseElementBehavior elem, IEnumerable<TemporalZoneBehavior> zones, bool animated) {

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
        var changed = ageBehavior.SetCurrentAge(ageBehavior.RealAge + totalAgeShift, animated, DURATION_ANIM_FROM_AGE_TO_AGE_SEC);
        if (changed) {
            PushElementsAboveIfNecessary(elem, ageBehavior);
            return true;
        }

        return false;
    }

    void PushElementsAboveIfNecessary(BaseElementBehavior elem, AgeBehavior ageBehavior) {

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
            e.TryMoveUp(diff, DURATION_ANIM_FROM_AGE_TO_AGE_SEC);
        }
    }

}
