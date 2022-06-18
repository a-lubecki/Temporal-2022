using UnityEngine;


[CreateAssetMenu(fileName = "DataCharacterInChapter", menuName = "Scriptable Objects/Character in chapter")]
public class DataCharacterInChapter : ScriptableObject {

    [field: SerializeField] public DataCharacter DataCharacter { get; protected set; }

    [field: SerializeField] public int InitialAge { get; protected set; }
    [field: SerializeField] public Team Team { get; protected set; }
    [field: SerializeField] public DataTemporalAbility Ability { get; protected set; }

}
