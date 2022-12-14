using System;
using UnityEngine;


[CreateAssetMenu(fileName = "DataChapter", menuName = "Scriptable Objects/Chapter")]
public class DataChapter : ScriptableObject {


    [field: SerializeField] public string ChapterName { get; protected set; }
    [field: SerializeField, TextArea] public string TextChapterStory { get; protected set; }
    [SerializeField] DataLevel[] dataLevel;


    public bool HasLevel(int levelNumber) {
        var pos = levelNumber - 1;
        return pos >= 0 && pos < dataLevel.Length;
    }

    DataLevel GetLevel(int levelNumber) {

        if (!HasLevel(levelNumber)) {
            throw new InvalidOperationException("Missing level : " + levelNumber);
        }

        return dataLevel[levelNumber - 1];
    }

    public string GetTextStory(int levelNumber) {
        return GetLevel(levelNumber).textStory;
    }

    public AudioClip GetMusic(int levelNumber) {
        return GetLevel(levelNumber).audioClipMusic;
    }

    public AudioClip GetAmbience(int levelNumber) {
        return GetLevel(levelNumber).audioClipAmbience;
    }

    public GameObject GetLevelPrefab(int levelNumber) {
        return GetLevel(levelNumber).prefab;
    }

    public DataAITeam GetDataAINeutral(int levelNumber) {
        return GetLevel(levelNumber).aiNeutral;
    }

    public DataAITeam GetDataAIEnemy(int levelNumber) {
        return GetLevel(levelNumber).aiEnemy;
    }

}


[Serializable]
class DataLevel {

    public GameObject prefab;
    public AudioClip audioClipMusic;
    public AudioClip audioClipAmbience;
    [TextArea(5, 10)] public string textStory;
    public DataAITeam aiNeutral;
    public DataAITeam aiEnemy;

}

[Serializable]
public class DataAITeam {

    [SerializeField] float maxMovementsPerTurn = 1;
    [SerializeField] int maxMovementsPerCharacter = 1;

    public float MaxMovementsPerTurn => maxMovementsPerTurn;
    public int MaxMovementsPerCharacter => maxMovementsPerCharacter;

}
