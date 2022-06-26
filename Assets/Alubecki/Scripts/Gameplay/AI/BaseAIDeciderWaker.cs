using UnityEngine;
using UnityEngine.Events;


public abstract class BaseAIDeciderWaker : MonoBehaviour {


    UnityEvent eventWakeUp = new UnityEvent();


    protected virtual void OnDrawGizmos() {

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    protected void WakeUpDeciders() {
        eventWakeUp.Invoke();
    }

    public void AttachDecider(BaseAIDecider decider) {
        eventWakeUp.AddListener(decider.WakeUpDecider);
    }

}