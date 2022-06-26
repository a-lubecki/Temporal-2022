using UnityEngine;


public class EnvironmentObjectBehavior : BaseElementBehavior {


    AgeBehavior ageBehavior;//can be null if no age

    [SerializeField] string objectName;
    [SerializeField, TextArea] string objectDescription;


    public override DisplayableCharacteristics DisplayableCharacteristics => new DisplayableCharacteristics(
        new Color(0.8f, 0.8f, 0.8f),
        objectName,
        GetObjectDescription() + GetAgeText()
    );

    string GetObjectDescription() {
        return string.IsNullOrEmpty(objectDescription) ? "" : objectDescription + "\n\n";
    }

    string GetAgeText() {
        return (ageBehavior == null) ? "" : ageBehavior.DisplayableText + "\n";
    }

    protected virtual void Awake() {
        
        ageBehavior = GetComponent<AgeBehavior>();
    }

}
