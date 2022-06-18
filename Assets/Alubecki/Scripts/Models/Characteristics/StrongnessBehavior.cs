using UnityEngine;


public class StrongnessBehavior : MonoBehaviour {


    [SerializeField] Strongness strongness;

    public bool CanMove(Weight weight) {
        return (int)strongness > (int)weight;
    }

    public bool CanMove(CumulatedWeight cumulatedWeight) {
        return CanMove(cumulatedWeight.totalWeight);
    }

    public void UpdateStrongness(Strongness strongness) {
        this.strongness = strongness;
    }

}


public enum Strongness : int {

    WEAK = 0,
    STRONG = 3,
    POWERFUL = 9
}
