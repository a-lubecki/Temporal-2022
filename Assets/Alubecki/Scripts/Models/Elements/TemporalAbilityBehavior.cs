using System;
using UnityEngine;


[DisallowMultipleComponent]
public class TemporalAbilityBehavior : MonoBehaviour {


    [SerializeField] DataTemporalAbility temporalAbility;

    public DataTemporalAbility TemporalAbility => temporalAbility;
    public string DisplayableText => "<b>Ability:</b> " + temporalAbility.AbilityName + "\n" + temporalAbility.AbilityDescription;


    public void InitData(DataTemporalAbility temporalAbility) {
        this.temporalAbility = temporalAbility ?? throw new ArgumentException("TemporalAbility cannot be null");
    }

}
