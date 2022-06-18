using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class InGameControlsBehavior : BaseControlsBehavior {


    //if pointer is already on UI elements, it's used for disabling raycasts for objects selection
    bool isPointerOverUI;


    protected override string CurrentActionMapName => "InGame";

    public bool MustPauseGame { get; private set; }
    public bool MustRotateCameraLeft { get; private set; }
    public bool MustRotateCameraRight { get; private set; }
    public bool MustDezoom { get; private set; }
    public bool MustZoom { get; private set; }
    public Vector2 PointerPosition { get; private set; }
    public bool MustNavigatePointer { get; private set; }
    public bool MustValidateSelection { get; private set; }
    public bool MustCancelSelection { get; private set; }
    public bool MustHideInterface { get; private set; }
    public bool MustShowInterface { get; private set; }
    public bool MustGoToPreviousNextMovement { get; private set; }
    public bool IsPreviousMovement { get; private set; }


    protected override void Update() {

        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    protected override void ResetControlsValues() {
        //reset values to not keep actions status
        MustPauseGame = false;
        MustRotateCameraLeft = false;
        MustRotateCameraRight = false;
        MustDezoom = false;
        MustZoom = false;
        PointerPosition = Vector3.zero;
        MustNavigatePointer = false;
        MustValidateSelection = false;
        MustCancelSelection = false;
        MustHideInterface = false;
        MustShowInterface = false;
        MustGoToPreviousNextMovement = false;
    }

    //callback from PlayerInput
    void OnPauseGame() {
        MustPauseGame = true;
    }

    //callback from PlayerInput
    void OnMoveCamera(InputValue v) {

        var value = v.Get<Vector2>();
        if (value.x < 0) {
            MustRotateCameraLeft = true;
        } else if (value.x > 0) {
            MustRotateCameraRight = true;
        } else if (value.y < 0) {
            MustDezoom = true;
        } else if (value.y > 0) {
            MustZoom = true;
        }
    }

    //callback from PlayerInput
    void OnNavigatePointer(InputValue v) {

        PointerPosition = v.Get<Vector2>();

        if (!isPointerOverUI) {
            MustNavigatePointer = true;
        }
    }

    //callback from PlayerInput
    void OnSelect(InputValue v) {

        PointerPosition = v.Get<Vector2>();

        if (!isPointerOverUI) {
            MustValidateSelection = true;
            MustNavigatePointer = true;
        }
    }

    //callback from PlayerInput
    void OnDeselect() {

        MustCancelSelection = true;

        if (!isPointerOverUI) {
            MustNavigatePointer = true;
        }
    }

    //callback from PlayerInput
    void OnHideInterface(InputValue v) {

        var value = v.Get<float>();
        if (value > 0) {
            MustHideInterface = true;
        } else {
            MustShowInterface = true;
        }
    }

    //callback from PlayerInput
    void OnPreviousNextMovement(InputValue v) {
        MustGoToPreviousNextMovement = true;
        IsPreviousMovement = (v.Get<float>() < 0);
    }

}
