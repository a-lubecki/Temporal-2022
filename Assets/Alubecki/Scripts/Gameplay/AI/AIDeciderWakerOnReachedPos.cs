using UnityEngine;


/// <summary>
/// Put this script on a game object.
/// If the invoker object reach this object, it triggers the event once.
/// </summary>
public class AIDeciderWakerOnReachedPos : BaseAIDeciderWaker {


    [SerializeField] Transform trInvoker;

    bool didInvokeOnce = false;


    void Update() {

        if (didInvokeOnce) {
            return;
        }

        if (trInvoker.transform.position == transform.position) {
            didInvokeOnce = true;
            WakeUpDeciders();
        }
    }

}
