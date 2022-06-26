using UnityEngine;


[DisallowMultipleComponent]
public abstract class BaseAIDecider : MonoBehaviour {

    public abstract BaseMovement NewMovement(CharacterBehavior character);

}

///TYPES :
// path
// specific : ex, si moins de 2 cases du joueur = suivre
