using System.Collections.Generic;
using UnityEngine;


public static class ListExtensions {

    public static void Shuffle<T>(this List<T> list) {

        int n = list.Count;

        while (n > 1) {

            var k = Random.Range(0, n);

            n--;

            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T PickRandom<T>(this List<T> list) {

        if (list.Count <= 0) {
            return default;
        }

        return list[Random.Range(0, list.Count)];
    }



}
