using UnityEngine;


[DisallowMultipleComponent]
public abstract class BaseAIDecider : MonoBehaviour {

    /// <summary>
    /// If there is a trigger, it must be invoked to wake up the decider.
    /// If there is no trigger, the decider automatically wakes up at startup.
    /// </summary>
    [SerializeField] BaseAIDeciderWaker waker;

    public bool IsSleeping { get; private set; }


    protected virtual void Awake() {

        IsSleeping = (waker != null);
        waker?.AttachDecider(this);
    }

    public void WakeUpDecider() {
        IsSleeping = false;
    }

    public abstract BaseMovement NewMovement(CharacterBehavior character);

}
