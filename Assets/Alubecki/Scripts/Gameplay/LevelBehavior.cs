using System;
using UnityEngine;


[DisallowMultipleComponent]
public class LevelBehavior : MonoBehaviour {


    DataChapter dataChapter;


    public bool IsFirstLevelOfChapter { get; private set; }
    public int LevelNumber { get; private set; }
    public string TextStory { get; protected set; }


    public void InitLevel(DataChapter dataChapter, int levelNumber) {

        this.dataChapter = dataChapter ?? throw new ArgumentException();

        IsFirstLevelOfChapter = levelNumber == 1;
        LevelNumber = levelNumber;
        TextStory = dataChapter.GetTextStory(levelNumber);
    }

}
