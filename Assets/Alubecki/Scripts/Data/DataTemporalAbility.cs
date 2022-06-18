using UnityEngine;


[CreateAssetMenu(fileName = "DataTemporalAbility", menuName = "Scriptable Objects/Temporal Ability")]
public class DataTemporalAbility : ScriptableObject {

    [field: SerializeField] public string AbilityName { get; protected set; }
    [field: SerializeField] public string AbilityDescription { get; protected set; }
    [field: SerializeField] public int AgeShiftYears { get; protected set; }
    [field: SerializeField] public Material ZoneMaterial { get; protected set; }

}
