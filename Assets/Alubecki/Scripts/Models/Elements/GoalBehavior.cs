using UnityEngine;


public class GoalBehavior : BaseElementBehavior {


    public override DisplayableCharacteristics DisplayableCharacteristics => new DisplayableCharacteristics(
        new Color(0.8f, 0.8f, 0.8f),
        "Goal",
        "Characters must go on it to finish the level."
    );

}
