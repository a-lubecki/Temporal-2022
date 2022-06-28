using System;
using System.Collections;
using System.Linq;
using UnityEngine;


public class GameManager : MonoBehaviour {


    [SerializeField] DataChapter dataChapter;
    [SerializeField] int nextLevelNumberToLoad = 1;

    [SerializeField] AudioClip audioClipWin;
    [SerializeField] AudioClip audioClipGameOverAmbience;
    [SerializeField] AudioClip audioClipUndo;
    [SerializeField] AudioClip audioClipRedo;

    public bool IsLevelLoaded { get; private set; }
    public LevelBehavior CurrentLevel => Game.Instance.boardBehavior.CurrentLevel;
    public int CurrentLevelNumber => CurrentLevel.LevelNumber;
    public bool IsFirstLevelOfChapter => CurrentLevel?.IsFirstLevelOfChapter ?? false;
    public bool MustShowChapter { get; private set; }
    public bool MustStartAfterShowingChapter => !Game.Instance.viewChapterBehavior.IsVisible;
    public bool IsLevelAnimating { get; private set; }
    public bool IsLevelShown { get; private set; }
    public bool IsLevelReadyToPlay => IsLevelShown && !IsLevelAnimating;
    public bool IsLevelReadyToFinish => !IsLevelShown && !IsLevelAnimating;
    public BaseElementBehavior SelectedElement => Game.Instance.elementsSelectionBehavior.SelectedElement;
    public bool IsElementSelected => Game.Instance.elementsSelectionBehavior.IsElementSelected;
    public bool IsNewElementSelected => Game.Instance.elementsSelectionBehavior.IsNewElementSelected;
    public bool IsNextMovementSelected => Game.Instance.movementsSelectionBehavior.IsNextMovementSelected;
    public bool IsNextMovementNeedResolving => Game.Instance.movementsSelectionBehavior?.NextMovement?.NeedsMovementResolving ?? false;
    public bool IsResolvingMovement => Game.Instance.movementResolver.IsResolvingMovement;
    public bool HasTriggerUndoRedo { get; private set; }
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
        Game.Instance.overlayHUD.Hide();
        Game.Instance.panelWin.Hide();
        Game.Instance.viewGameOverBehavior.Hide();
        Game.Instance.viewChapterBehavior.Hide();

        ZoomableOutline.areZoomableOutlinesEnabled = false;
    }

    void OnApplicationQuit() {
        //avoid calls from state graph when game exits, triggering errors logs
        gameObject.SetActive(false);
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

        MustShowChapter = (nextLevelNumberToLoad <= 1);

        IsLevelAnimating = false;
        IsLevelShown = false;

        IsGameOver = false;
        IsWin = false;

        Game.Instance.elementsSelectionBehavior.CancelSelection();

        //hide the board to avoid seeing the loaded level before its animation
        Game.Instance.boardBehavior.Hide();
        Game.Instance.mainCameraController.SetLookAtTarget(null);

        var loaded = Game.Instance.boardBehavior.LoadNewLevel(dataChapter, nextLevelNumberToLoad);
        if (!loaded) {

            Debug.Log("Level " + nextLevelNumberToLoad + " not found => end of chapter");

            //force quit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }

        //prepare next level for after
        nextLevelNumberToLoad++;

        //wait before setting level as loaded because the level Instantiate can slow down the level appearing animation
        StartCoroutine(SetLevelAsLoadedAfterDelay());
    }

    IEnumerator SetLevelAsLoadedAfterDelay() {

        yield return new WaitForSeconds(0.2f);

        if (CurrentLevel != null) {
            IsLevelLoaded = true;
        }
    }

    public void OnShowChapter() {

        Game.Instance.viewChapterBehavior.Show(dataChapter);
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

        ZoomableOutline.areZoomableOutlinesEnabled = true;

        Game.Instance.inGameControlsBehavior.EnableControlsAfterDelay(0.1f);

        Game.Instance.mementoCaretaker.SaveCurrentState();

        ComputeAndDisplayNextNPCMovementIndications();

        Game.Instance.panelLevelInfo.Show(dataChapter.ChapterName, CurrentLevelNumber);
        Game.Instance.overlayHUD.ShowFade();
    }

    public void OnSelectionStepBegin() {

        Game.Instance.selectionStepManager.enabled = true;

        //at first if there is only one character in level, select it automatically
        if (Game.Instance.elementsSelectionBehavior.LastSelectedElement == null) {

            var characters = Game.Instance.boardBehavior.GetElements().Where(e => e.TryGetComponent<CharacterBehavior>(out var character) && character.IsPlayable);
            if (characters.Count() == 1) {
                Game.Instance.elementsSelectionBehavior.ValidateSelection(characters.First(), false);
            } else {
                Game.Instance.mainCameraController.SetLookAtTarget(null);
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

        Game.Instance.inGameControlsBehavior.DisableControls();

        Game.Instance.cursorBehavior.Hide();

        Game.Instance.movementResolver.ExecuteCurrentMovement();
    }

    public void OnMovementResolveEnd() {

        Game.Instance.inGameControlsBehavior.EnableControls();

        Game.Instance.mementoCaretaker.SaveCurrentState();

        ComputeAndDisplayNextNPCMovementIndications();
    }

    public void OnMovementExecuteWithoutResolving() {

        Game.Instance.cursorBehavior.Hide();

        Game.Instance.movementResolver.ExecuteCurrentMovement();
    }

    public void OnAnimateLevelHideBegin() {

        Game.Instance.mementoCaretaker.Reset();

        Game.Instance.panelPileBehavior.Hide();
        Game.Instance.overlayHUD.HideFade();

        Game.Instance.elementsSelectionBehavior.CancelSelection();
        Game.Instance.inGameControlsBehavior.DisableControls();

        ZoomableOutline.areZoomableOutlinesEnabled = false;

        IsLevelAnimating = true;

        Game.Instance.audioManager.PlaySimpleSound(audioClipWin);

        StartCoroutine(AnimateLevelHideAfterDelay());
    }

    IEnumerator AnimateLevelHideAfterDelay() {

        yield return new WaitForSeconds(0.1f);

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
    }

    public void OnLevelFinish() {

        Game.Instance.boardBehavior.UnloadCurrentLevel();

        IsLevelLoaded = false;

        //reset win camera of the previous level to move to the main dolly camera
        Game.Instance.vcamWin.Priority = 0;
    }

    public void TriggerEnd(bool isGameOver) {

        IsGameOver = isGameOver;
        IsWin = !isGameOver;
    }

    public void OnGameOverStart() {

        IsGameOver = true;

        Game.Instance.viewGameOverBehavior.ShowFade();
        Game.Instance.overlayHUD.HideFade();

        CurrentLevel.StopMusic();
        Game.Instance.audioManager.PlayAmbience(audioClipGameOverAmbience);
    }

    public void OnGameOverFinish() {

        IsGameOver = false;

        CurrentLevel.PlayMusic();

        Game.Instance.viewGameOverBehavior.HideFade();
        Game.Instance.overlayHUD.ShowFade();

        //prevent from redoing the same move that leads to gameover
        Game.Instance.mementoCaretaker.ClearHistoryAfterCursor();
    }

    public void TryUndoRedoMovement(bool isUndo) {

        if (isUndo) {

            if (Game.Instance.mementoCaretaker.Undo()) {
                Game.Instance.audioManager.PlaySimpleSound(audioClipUndo);
                HasTriggerUndoRedo = true;
            }

        } else {

            if (Game.Instance.mementoCaretaker.Redo()) {
                Game.Instance.audioManager.PlaySimpleSound(audioClipRedo);
                HasTriggerUndoRedo = true;
            }
        }
    }

    public void OnUndoRedoMovement() {

        HasTriggerUndoRedo = false;

        var previousSelected = Game.Instance.elementsSelectionBehavior.SelectedElement;
        Game.Instance.elementsSelectionBehavior.CancelSelection(false);
        Game.Instance.elementsSelectionBehavior.ClearLastSelected();

        //override look at transform to avoid a weird camera behavior
        Game.Instance.mainCameraController.SetLookAtTarget(previousSelected?.transform);

        Game.Instance.cursorBehavior.Hide();

        Game.Instance.movementsSelectionBehavior.ClearNextMovement();

        ComputeAndDisplayNextNPCMovementIndications();
    }

    void ComputeAndDisplayNextNPCMovementIndications() {

        Game.Instance.aiTeamNeutral.ComputeNextNPCsMovements();
        Game.Instance.aiTeamEnemy.ComputeNextNPCsMovements();

        Game.Instance.aiTeamNeutral.DisplayNPCMovementIndications();
        Game.Instance.aiTeamEnemy.DisplayNPCMovementIndications();
    }

}
