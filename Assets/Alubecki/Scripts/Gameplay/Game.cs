using Cinemachine;
using Lean.Pool;
using UnityEngine;


public class Game : MonoBehaviourSingleton<Game> {


    public GameManager gameManager;
    public LevelAnimator levelAnimator;
    public MenuControlsBehavior menuControlsBehavior;
    public InGameControlsBehavior inGameControlsBehavior;
    public SelectionStepManager selectionStepManager;
    public MovementStepManager movementStepManager;
    public ElementsSelectionBehavior elementsSelectionBehavior;
    public MovementsSelectionBehavior movementsSelectionBehavior;
    public MovementResolver movementResolver;
    public MementoCaretaker mementoCaretaker;

    public AITeamBehavior aiTeamNeutral;
    public AITeamBehavior aiTeamEnemy;

    public CameraController mainCameraController;
    public CinemachineVirtualCamera vcamWin;

    public LeanGameObjectPool poolIndicationCharacterMove;
    public LeanGameObjectPool poolIndicationMovableObjectMove;
    public LeanGameObjectPool poolIndicationNPCMovement;
    public LeanGameObjectPool poolPanelElement;
    public LeanGameObjectPool poolActionButtonGroup;
    public LeanGameObjectPool poolTemporalZone;

    public CursorBehavior cursorBehavior;
    public CursorSelectedBehavior cursorSelectedBehavior;
    public IndicationsSpawnerBehavior indicationsSpawnerBehavior;
    public ActionButtonsSpawnerBehavior actionButtonsSpawnerBehavior;
    public TemporalityManager temporalityManager;
    public BoardBehavior boardBehavior;

    public Canvas canvas;
    public BaseViewOverlay overlayHUD;
    public PanelLevelInfo panelLevelInfo;
    public PanelPileBehavior panelPileBehavior;
    public PanelWin panelWin;
    public ViewGameOverBehavior viewGameOverBehavior;
    public ViewChapterBehavior viewChapterBehavior;

    public AudioManager audioManager;

}
