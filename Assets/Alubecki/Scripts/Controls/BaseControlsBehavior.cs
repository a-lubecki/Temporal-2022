using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public abstract class BaseControlsBehavior : MonoBehaviour {


    protected virtual string CurrentActionMapName { get; }

    PlayerInput playerInput;
    Coroutine coroutineDelayEnable;


    protected virtual void Awake() {
        playerInput = GetComponent<PlayerInput>();
    }

    protected virtual void Update() {
        //override if necessary
    }

    protected virtual void LateUpdate() {
        ResetControlsValues();
    }

    protected abstract void ResetControlsValues();

    public void SelectCurrentActionMap() {

        playerInput.SwitchCurrentActionMap(CurrentActionMapName);

        Debug.Log("Selected action map : " + CurrentActionMapName);
    }

    public void DisableControls() {

        StopCoroutineDelayEnable();

        playerInput.actions.Disable();

        Debug.Log("Disabled controls");
    }

    public void EnableControls() {

        playerInput.actions.Enable();

        Debug.Log("Enable controls");
    }

    public void DisableControlsForSeconds(float timeSec) {

        DisableControls();

        coroutineDelayEnable = StartCoroutine(EnableControlsShifted(timeSec));
    }

    public void EnableControlsAfterDelay(float delaySec) {

        StopCoroutineDelayEnable();

        coroutineDelayEnable = StartCoroutine(EnableControlsShifted(delaySec));
    }

    void StopCoroutineDelayEnable() {

        //stop coroutine avoid bugs if coroutine runs and we want to disable completely the controls
        //then it won't enable controls again automatically
        if (coroutineDelayEnable != null) {
            StopCoroutine(coroutineDelayEnable);
        }
    }

    IEnumerator EnableControlsShifted(float delaySec) {

        yield return new WaitForSeconds(delaySec);

        EnableControls();

        coroutineDelayEnable = null;
    }

}
