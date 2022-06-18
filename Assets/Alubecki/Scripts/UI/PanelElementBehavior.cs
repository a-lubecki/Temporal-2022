using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class PanelElementBehavior : MonoBehaviour {


    Image imageBackground;
    [SerializeField] TextMeshProUGUI textTitle;
    [SerializeField] TextMeshProUGUI textCharacteristics;


    void Awake() {
        imageBackground = GetComponent<Image>();
    }

    public void UpdateUI(DisplayableCharacteristics characteristics) {

        if (characteristics == null) {
            throw new ArgumentException();
        }

        var color = characteristics.PanelColor;
        var alpha = imageBackground.color.a;
        imageBackground.color = new Color(color.r, color.g, color.b, alpha);

        textTitle.text = characteristics.TextElementTitle;
        textCharacteristics.text = characteristics.TextCharacteristics;
    }

}

