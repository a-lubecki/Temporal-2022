using System.Collections;
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

    public IEnumerator ResolveTemporality() {

        var isAnimatingZones = false;
        var zones = GetZones();
        var agedElements = Game.Instance.boardBehavior.GetElements().Where(e => e.TryGetComponent<AgeBehavior>(out _));

        foreach (var elem in agedElements) {

            var totalAgeShiftYears = 0;

            //get influence of all zones
            foreach (var zone in zones) {

                if (zone.IsOwner(elem)) {
                    //the character is not impacted by its zone
                    continue;
                }

                if (zone.ContainsElement(elem)) {
                    totalAgeShiftYears += zone.AgeShiftYears;
                }
            }

            var ageBehavior = elem.GetComponent<AgeBehavior>();
            var changed = ageBehavior.SetAgeShift(totalAgeShiftYears, true, DURATION_ANIM_FROM_AGE_TO_AGE_SEC);
            if (changed) {
                isAnimatingZones = true;

                PushElementsAboveIfNecessary(elem, ageBehavior);
            }
        }

        if (isAnimatingZones) {
            yield return new WaitForSeconds(DURATION_ANIM_FROM_AGE_TO_AGE_SEC + 0.1f);
        }
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
