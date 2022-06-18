using UnityEngine;


public class GoalBehavior : BaseElementBehavior {


    public override DisplayableCharacteristics DisplayableCharacteristics => new DisplayableCharacteristics(
        Color.grey,
        "Goal",
        "Characters must go on it to finish the level."
    );

}
