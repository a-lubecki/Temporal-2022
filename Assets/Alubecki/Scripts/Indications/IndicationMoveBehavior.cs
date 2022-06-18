using System;


public class IndicationMoveBehavior : GridPosBehavior {


    public BaseMovement Movement { get; private set; }


    void OnDisable() {
        ClearMovement();
    }

    public void SetMovement(BaseMovement movement) {

        if (movement == null) {
            throw new ArgumentNullException();
        }

        this.Movement = movement;
    }

    public void ClearMovement() {
        Movement = null;
    }

}
