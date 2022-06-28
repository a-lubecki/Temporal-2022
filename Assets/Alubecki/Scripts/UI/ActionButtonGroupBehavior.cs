using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ActionButtonGroupBehavior : MonoBehaviour {


    CanvasScaler canvasScaler;
    [SerializeField] GameObject goButtonFar;
    [SerializeField] TextMeshProUGUI textButtonNear;
    [SerializeField] TextMeshProUGUI textButtonFar;

    Vector3 characterPos;
    Vector3 actionableElemPos;
    BaseMovement actionNear;
    BaseMovement actionFar;


    void Awake() {
        canvasScaler = Game.Instance.canvas.GetComponent<CanvasScaler>();
    }

    void OnDisable() {

        actionNear = null;
        actionFar = null;
    }

    void Update() {

        if (actionNear == null && actionFar == null) {
            //no pos to update
            return;
        }

        //the buttons group is displayed on top square of the target block (so +1 in y)
        var camPointCharacter = Camera.main.WorldToScreenPoint(characterPos + Vector3.up);
        var camPointActionableElem = Camera.main.WorldToScreenPoint(actionableElemPos);

        //take canvas scaler into account to accurately place the button group
        var match = canvasScaler.matchWidthOrHeight;
        var offset = (Screen.width / canvasScaler.referenceResolution.x) * (1 - match) + (Screen.height / canvasScaler.referenceResolution.y) * match;
        transform.localPosition = camPointActionableElem / offset;

        //check if group is up or down depending of the vertical diff on screen between character and actionable object
        var rot = (camPointActionableElem.y >= camPointCharacter.y) ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
        transform.localRotation = rot;
        textButtonNear.transform.localRotation = rot;
        textButtonFar.transform.localRotation = rot;
    }

    public void UpdateUI(Vector3 actionableElemPos, Vector3 characterPos, BaseMovement actionOneTime, BaseMovement actionStateChange) {

        if (actionOneTime == null && actionStateChange == null) {
            throw new ArgumentException("Must have at least one action to show a button group");
        }

        this.actionableElemPos = actionableElemPos;
        this.characterPos = characterPos;

        this.actionNear = actionOneTime != null ? actionOneTime : actionStateChange;
        this.actionFar = actionStateChange;

        textButtonNear.text = actionNear?.DisplayableName;
        textButtonFar.text = actionFar?.DisplayableName;

        //if only one button
        if (actionOneTime == null || actionStateChange == null) {
            goButtonFar.SetActive(false);
        }
    }

    public void OnClickActionButtonNear() {
        Game.Instance.movementsSelectionBehavior.ValidateNextMovement(actionNear);
    }

    public void OnClickActionButtonFar() {
        Game.Instance.movementsSelectionBehavior.ValidateNextMovement(actionFar);
    }

    public BaseMovement FindDisplayedMovement<T>() where T : BaseMovement {

        if (actionNear != null && actionNear is T) {
            return actionNear;
        }

        if (actionFar != null && actionFar is T) {
            return actionFar;
        }

        //not found
        return null;
    }

}
