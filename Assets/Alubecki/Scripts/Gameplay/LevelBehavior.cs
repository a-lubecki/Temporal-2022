using System;
using UnityEngine;


[DisallowMultipleComponent]
public class LevelBehavior : MonoBehaviour {


    DataChapter dataChapter;


    public bool IsFirstLevelOfChapter { get; private set; }
    public int LevelNumber { get; private set; }
    public string TextStory => dataChapter.GetTextStory(LevelNumber);


    public void InitLevel(DataChapter dataChapter, int levelNumber) {

        this.dataChapter = dataChapter ?? throw new ArgumentException();

        IsFirstLevelOfChapter = levelNumber == 1;
        LevelNumber = levelNumber;

        Game.Instance.aiTeamNeutral.InitAITeam(dataChapter.GetDataAINeutral(levelNumber));
        Game.Instance.aiTeamEnemy.InitAITeam(dataChapter.GetDataAIEnemy(levelNumber));

        PlayMusic();
    }

    public void PlayMusic() {

        Game.Instance.audioManager.PlayMusic(dataChapter.GetMusic(LevelNumber));
        Game.Instance.audioManager.PlayAmbience(dataChapter.GetAmbience(LevelNumber));
    }

    public void StopMusic() {

        Game.Instance.audioManager.PlayMusic(null);
        Game.Instance.audioManager.PlayAmbience(null);
    }

}
