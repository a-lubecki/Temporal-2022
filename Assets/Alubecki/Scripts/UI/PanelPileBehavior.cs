using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using UnityEngine;
using UnityEngine.UI;


public class PanelPileBehavior : MonoBehaviour {


    Transform trContent;


    private void Awake() {
        trContent = transform.GetChild(0);
    }

    public void Show(IEnumerable<BaseElementBehavior> pile) {

        Hide();

        var elemsToShow = pile.Where(e => e.HasDisplayableCharacteristics);

        if (elemsToShow.Count() <= 0) {
            //no elems to display
            return;
        }

        trContent.gameObject.SetActive(true);

        foreach (var elem in elemsToShow) {

            var goPanelElem = Game.Instance.poolPanelElement.Spawn(trContent);
            goPanelElem.transform.SetAsFirstSibling();//put on top

            var panelElem = goPanelElem.GetComponent<PanelElementBehavior>();
            panelElem.UpdateUI(elem.DisplayableCharacteristics);
        }

        //force update panel to avoid resizing bugs
        LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }

    public void Hide() {

        foreach (var panel in GetComponentsInChildren<PanelElementBehavior>()) {

            LeanPool.Despawn(panel);
        }

        trContent.gameObject.SetActive(false);
    }

}
