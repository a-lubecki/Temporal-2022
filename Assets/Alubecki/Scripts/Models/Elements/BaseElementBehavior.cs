using System.Collections.Generic;
using UnityEngine;


public abstract class BaseElementBehavior : GridPosBehavior {


    [SerializeField] bool preventFromWalkingOn;

    public virtual bool IsCursorAboveElement => true;
    public virtual bool CanBeSelected => false;
    public int ColliderHeight => GetComponentInChildren<OverridenHeightBehavior>()?.ColliderHeight ?? 1;
    public virtual bool IsPhysicallyWalkableInBlock => !preventFromWalkingOn && ColliderHeight <= 0;
    public virtual bool IsPhysicallyWalkableOverBlock => !preventFromWalkingOn && ColliderHeight > 0;
    public virtual bool CanMove => false;
    public virtual bool CanFall => false;
    protected virtual BaseMovement.Factory[] PossibleMovements => null;

    public bool HasDisplayableCharacteristics => DisplayableCharacteristics != null;
    public abstract DisplayableCharacteristics DisplayableCharacteristics { get; }


    public int GetColliderHeightForAge(int age) {
        return GetComponent<AgeBehavior>()?.FindMeshForAge(age)?.GetComponent<OverridenHeightBehavior>()?.ColliderHeight ?? 1;
    }

    public IEnumerable<BaseMovement.Factory> GetPossibleMovements() {

        if (PossibleMovements == null) {
            return null;
        }

        //defensive copy
        return new List<BaseMovement.Factory>(PossibleMovements);
    }

}


public class DisplayableCharacteristics {

    public Color PanelColor { get; private set; }
    public string TextElementTitle { get; private set; }
    public string TextCharacteristics { get; private set; }

    public DisplayableCharacteristics(Color panelColor, string textElementTitle, string textCharacteristics) {
        PanelColor = panelColor;
        TextElementTitle = textElementTitle;
        TextCharacteristics = textCharacteristics;
    }

}
