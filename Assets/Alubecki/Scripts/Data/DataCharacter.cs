using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DataCharacter", menuName = "Scriptable Objects/Character")]
public class DataCharacter : ScriptableObject {


    [field: SerializeField] public CharacterBehavior CharacterPrefab { get; protected set; }

    [field: SerializeField] public string FullName { get; protected set; }
    [field: SerializeField, TextArea] public string Description { get; protected set; }
    [field: SerializeField] public SpeciesType SpeciesType { get; protected set; }
    [field: SerializeField] public Gender Gender { get; protected set; }
    [field: SerializeField] public int MaxAge { get; protected set; }

    [SerializeField] Tuple<int, Height>[] heightByAge;
    [SerializeField] Tuple<int, Weight>[] weightByAge;
    [SerializeField] Tuple<int, Agility>[] agilityByAge;
    [SerializeField] Tuple<int, Strongness>[] strongnessByAge;

    public Dictionary<int, Height> HeightByAge => ToDictionary(heightByAge);
    public Dictionary<int, Weight> WeightByAge => ToDictionary(weightByAge);
    public Dictionary<int, Agility> AgilityByAge => ToDictionary(agilityByAge);
    public Dictionary<int, Strongness> StrongnessByAge => ToDictionary(strongnessByAge);

    [field: SerializeField] public AudioClip AudioClipRotate { get; protected set; }
    [field: SerializeField] public AudioClip AudioClipMove { get; protected set; }
    [field: SerializeField] public AudioClip AudioClipClimb { get; protected set; }
    [field: SerializeField] public AudioClip AudioClipFall { get; protected set; }


    public static Dictionary<K, V> ToDictionary<K, V>(Tuple<K, V>[] tupleArray) {

        if (tupleArray == null) {
            return new Dictionary<K, V>();
        }

        var res = new Dictionary<K, V>();
        foreach (var tuple in tupleArray) {
            res.Add(tuple.key, tuple.value);
        }

        return res;
    }

}


[Serializable]
public struct Tuple<K, V> {
    public K key;
    public V value;
}
