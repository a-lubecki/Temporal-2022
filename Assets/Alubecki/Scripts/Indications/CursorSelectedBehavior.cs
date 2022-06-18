using UnityEngine;


public class CursorSelectedBehavior : MonoBehaviour {


    public void Show(BaseElementBehavior elem) {

        gameObject.SetActive(true);
        transform.localPosition = elem.transform.localPosition;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

}
