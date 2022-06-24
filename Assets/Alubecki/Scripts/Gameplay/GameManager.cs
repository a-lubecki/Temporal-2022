using System;
using System.Collections;
using System.Linq;
using UnityEngine;


public class GameManager : MonoBehaviour {


    [SerializeField] DataChapter dataChapter;
    [SerializeField] int nextLevelNumberToLoad = 1;

    [SerializeField] AudioClip audioClipWin;
    [SerializeField] AudioClip audioClipGameOverAmbience;

    public bool IsLevelLoaded => CurrentLevel != null;
    public LevelBehavior CurrentLevel => Game.Instance.boardBehavior.CurrentLevel;
    public int CurrentLevelNumber => CurrentLevel.LevelNumber;
    public bool IsFirstLevelOfChapter => CurrentLevel?.IsFirstLevelOfChapter ?? false;
    public bool IsLevelAnimating { get; private set; }
    public bool IsLevelShown { get; private set; }
    public bool IsLevelReadyToPlay => IsLevelShown && !IsLevelAnimating;
    public bool IsLevelReadyToFinish => !IsLevelShown && !IsLevelAnimating;
    public BaseElementBehavior SelectedElement => Game.Instance.elementsSelectionBehavior.SelectedElement;
    public bool IsElementSelected => Game.Instance.elementsSelectionBehavior.IsElementSelected;
    public bool IsNewElementSelected => Game.Instance.elementsSelectionBehavior.IsNewElementSelected;
    public bool IsNextMovementSelected => Game.Instance.movementsSelectionBehavior.IsNextMovementSelected;
    public bool IsResolvingMovement => Game.Instance.movementResolver.IsResolvingMovement;
    public bool IsGameOver { get; private set; }
    public bool IsWin { get; private set; }
    public bool MustStartNextLevel => Game.Instance.panelWin.MustStartNextLevel;


    public void Start() {

        Game.Instance.menuControlsBehavior.DisableControls();
        Game.Instance.inGameControlsBehavior.DisableControls();

        Game.Instance.selectionStepManager.enabled = false;
        Game.Instance.movementStepManager.enabled = false;

        Game.Instance.cursorBehavior.Hide();
        Game.Instance.cursorSelectedBehavior.Hide();
        Game.Instance.boardBehavior.Hide();

        Game.Instance.panelPileBehavior.Hide();
        Game.Instance.panelLevelInfo.Hide();
        Game.Instance.panelWin.Hide();
        Game.Instance.viewGameOverBehavior.Hide();
    }

    void OnApplicationQuit() {
        //avoid calls from state graph when game exits, triggering errors logs
        gameObject.SetActive(false);
    }

    public void TriggerEnd(bool isGameOver) {

        if (IsGameOver) {

            IsGameOver = true;
            IsWin = false;

            Debug.Log("END: GAME OVER");

        } else {

            IsGameOver = false;
            IsWin = true;

            Debug.Log("END: WIN");
        }
    }

    public void OnMainMenuStart() {

        Game.Instance.menuControlsBehavior.SelectCurrentActionMap();
        Game.Instance.menuControlsBehavior.EnableControls();
    }

    public void OnMainMenuFinish() {

        Game.Instance.menuControlsBehavior.DisableControls();
    }

    public void OnLevelStart() {

        Game.Instance.inGameControlsBehavior.SelectCurrentActionMap();
        Game.Instance.inGameControlsBehavior.DisableControls();

        IsLevelAnimating = false;
        IsLevelShown = false;

        IsGameOver = false;
        IsWin = false;

        Game.Instance.elementsSelectionBehavior.CancelSelection();

        //hide the board to avoid seeing the loaded level before its animation
        Game.Instance.boardBehavior.Hide();

        var loaded = Game.Instance.boardBehavior.LoadNewLevel(dataChapter, nextLevelNumberToLoad);
        if (!loaded) {
            throw new NotSupportedException("Level " + nextLevelNumberToLoad + " found to play => end of chapter ?");
        }

        //prepare next level for after
        nextLevelNumberToLoad++;

        Game.Instance.panelLevelInfo.Show(dataChapter.ChapterName, CurrentLevelNumber);
    }

    public void OnAnimateLevelShowBegin() {

        IsLevelShown = true;
        IsLevelAnimating = true;

        Game.Instance.boardBehavior.Show();

        Game.Instance.levelAnimator.AnimateLevelShow(
            Game.Instance.boardBehavior.transform,
            CurrentLevel.transform,
            () => {

                Game.Instance.temporalityManager.InitZones();

                //hide the previous level story text
                Game.Instance.panelWin.Hide();

                Game.Instance.movementResolver.InitLevel(() => IsLevelAnimating = false);
            }
        );
    }

    public void OnAnimateLevelShowEnd() {

        Game.Instance.inGameControlsBehavior.EnableControlsAfterDelay(0.1f);

        Game.Instance.mementoCaretaker.SaveCurrentState();
    }

    public void OnSelectionStepBegin() {

        //Debug.Log(">> STEP selection");

        Game.Instance.selectionStepManager.enabled = true;

        //at first if there is only one character in level, select it automatically
        if (Game.Instance.elementsSelectionBehavior.LastSelectedElement == null) {

            var characters = Game.Instance.boardBehavior.GetElements().Where(e => e.TryGetComponent<CharacterBehavior>(out var character) && character.IsPlayable);
            if (characters.Count() == 1) {
                Game.Instance.elementsSelectionBehavior.ValidateSelection(characters.First(), false);
            }

        } else if (Game.Instance.elementsSelectionBehavior.IsNewElementSelected) {

            //coming from the movement state, manage reselection of another element
            Game.Instance.elementsSelectionBehavior.ValidateSelection(Game.Instance.elementsSelectionBehavior.SelectedElement);

            //avoid an infinite loop between selection and movement steps
            Game.Instance.elementsSelectionBehavior.ClearLastSelected();

        }
    }

    public void OnSelectionStepEnd() {

        Game.Instance.selectionStepManager.enabled = false;
    }

    public void OnMovementStepBegin() {

        //Debug.Log(">> STEP movement");

        Game.Instance.movementStepManager.enabled = true;

        //replace default cursor marker by the one of the selected cursor
        if (IsElementSelected) {
            Game.Instance.cursorSelectedBehavior.Show(SelectedElement);
        } else {
            Game.Instance.cursorSelectedBehavior.Hide();
        }

        //show or hide indications for next move
        Game.Instance.movementsSelectionBehavior.UpdateIndicationsAndActionButtons();
    }

    public void OnMovementStepEnd() {

        Game.Instance.movementStepManager.enabled = false;

        Game.Instance.cursorSelectedBehavior.Hide();

        Game.Instance.movementsSelectionBehavior.HideIndicationsAndActionButtons();
    }

    public void OnMovementResolveBegin() {

        //Debug.Log(">> Resolve movement");

        Game.Instance.inGameControlsBehavior.DisableControls();

        Game.Instance.cursorBehavior.Hide();

        Game.Instance.movementResolver.ResolveCurrentMovement();
    }

    public void OnMovementResolveEnd() {

        Game.Instance.inGameControlsBehavior.EnableControls();

        Game.Instance.movementsSelectionBehavior.ClearNextMovement();

        Game.Instance.mementoCaretaker.SaveCurrentState();
    }

    public void OnAnimateLevelHideBegin() {

        Game.Instance.mementoCaretaker.Reset();

        Game.Instance.panelPileBehavior.Hide();
        Game.Instance.elementsSelectionBehavior.CancelSelection();
        Game.Instance.inGameControlsBehavior.DisableControls();

        IsLevelAnimating = true;

        Game.Instance.audioManager.PlaySimpleSound(audioClipWin);

        StartCoroutine(AnimateLevelHideAfterDelay());
    }

    IEnumerator AnimateLevelHideAfterDelay() {

        yield return new WaitForSeconds(0.5f);

        Game.Instance.mainCameraController.ResetRotationAndZoom();

        yield return new WaitForSeconds(2);

        Game.Instance.levelAnimator.AnimateLevelHide(
            Game.Instance.boardBehavior.transform,
            CurrentLevel.transform,
            () => IsLevelAnimating = false
        );

        Game.Instance.temporalityManager.DeleteZones();
        Game.Instance.cursorBehavior.Hide();

        Game.Instance.panelWin.Show(CurrentLevel.TextStory);

        yield return new WaitForSeconds(0.8f);

        //switch to the camera aiming the panel
        Game.Instance.vcamWin.Priority = 100;
    }

    public void OnAnimateLevelHideEnd() {

        IsLevelAnimating = false;
        IsLevelShown = false;

        Game.Instance.inGameControlsBehavior.EnableControls();
    }

    public void OnLevelFinish() {

        Game.Instance.boardBehavior.UnloadCurrentLevel();

        Game.Instance.panelLevelInfo.Hide();

        //reset win camera of the previous level to move to the main dolly camera
        Game.Instance.vcamWin.Priority = 0;
    }

    public void OnGameOverStart() {

        IsGameOver = true;

        Game.Instance.viewGameOverBehavior.Show();

        CurrentLevel.StopMusic();
        Game.Instance.audioManager.PlayAmbience(audioClipGameOverAmbience);

        //TODO only enable "cancel previous movement" control
    }

    public void OnGameOverFinish() {

        IsGameOver = false;

        CurrentLevel.PlayMusic();

        Game.Instance.viewGameOverBehavior.Hide();
    }

}
