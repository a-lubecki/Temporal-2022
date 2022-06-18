using System;
using UnityEngine;


public class ViewGameOverBehavior : MonoBehaviour {


    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

}
