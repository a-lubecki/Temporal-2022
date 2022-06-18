using System;
using UnityEngine;


[CreateAssetMenu(fileName = "DataChapter", menuName = "Scriptable Objects/Chapter")]
public class DataChapter : ScriptableObject {


    [field: SerializeField] public string ChapterName { get; protected set; }
    [field: SerializeField, TextArea] public string TextChapterStory { get; protected set; }
    [field: SerializeField] public GameObject[] PrefabsLevel { get; protected set; }
    [field: SerializeField, TextArea] public string[] TextsStory { get; protected set; }


    public string GetTextStory(int levelNumber) {

        var pos = levelNumber - 1;
        if (pos < 0 || pos >= TextsStory.Length) {
            throw new InvalidOperationException("Missing story text for level : " + levelNumber);
        }

        return TextsStory[pos];
    }

    public bool HasLevel(int levelNumber) {
        var pos = levelNumber - 1;
        return pos >= 0 && pos < PrefabsLevel.Length;
    }

    public GameObject GetLevelPrefab(int levelNumber) {
        return PrefabsLevel[levelNumber - 1];
    }

}
