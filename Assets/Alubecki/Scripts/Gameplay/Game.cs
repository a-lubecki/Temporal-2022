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

    public LeanGameObjectPool poolIndicationCharacterMove;
    public LeanGameObjectPool poolIndicationMovableObjectMove;
    public LeanGameObjectPool poolIndicationNPCAction;
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
    public PanelPileBehavior panelPileBehavior;
    public ViewGameOverBehavior viewGameOverBehavior;

    public AudioSource audioSourceGlobalSounds;


}
