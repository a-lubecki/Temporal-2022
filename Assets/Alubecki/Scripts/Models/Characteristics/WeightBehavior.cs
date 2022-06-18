using System;
using System.Collections.Generic;
using UnityEngine;


public class WeightBehavior : MonoBehaviour {


    [SerializeField] Weight weight;
    public Weight Weight => weight;

    public string DisplayableText => "<b>Weight:</b> " + weight switch {
        Weight.NONE => "-",
        Weight.LIGHT => "Light",
        Weight.HEAVY => "Heavy",
        Weight.VERY_HEAVY => "Very Heavy",
        _ => throw new NotImplementedException()
    };

    public void UpdateWeight(Weight weight) {
        this.weight = weight;
    }
}

public enum Weight : int {

    NONE = 0,
    LIGHT = 1,
    HEAVY = 3,
    VERY_HEAVY = 9
}

public class CumulatedWeight {

    public readonly Weight totalWeight;

    public CumulatedWeight(Weight weight) {
        totalWeight = weight;
    }

    public CumulatedWeight(IEnumerable<Weight> weights) {

        var sum = 0;
        foreach (var w in weights) {
            sum += (int)w;
        }

        totalWeight = (sum >= (int)Weight.VERY_HEAVY) ? Weight.VERY_HEAVY
            : (sum >= (int)Weight.HEAVY) ? Weight.HEAVY
            : (sum >= (int)Weight.LIGHT) ? Weight.LIGHT
            : Weight.NONE;
    }

}
