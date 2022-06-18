using UnityEngine;


/// <summary>
/// Subclass used to take camera distance from level into account.
/// Originally, when the camera is far, the outline seems bigger than when the camera is close.
/// Use constantOutlineWidth instead of outlineWidth.
/// </summary>
public class ZoomableOutline : Outline {


    [SerializeField] float constantOutlineWidth = 1;

    private float lastThicknessMultiplier = 1;
    private float lastConstantOutlineWidth = 1;


    void LateUpdate() {

        var controller = Camera.main.GetComponent<CameraController>();
        if (controller != null) {

            var multiplier = controller.GetOutlinesThicknessMultiplier();
            if (multiplier != lastThicknessMultiplier || constantOutlineWidth != lastConstantOutlineWidth) {

                //change original outline width (parent attribute)
                OutlineWidth = constantOutlineWidth * multiplier;

                lastThicknessMultiplier = multiplier;
                lastConstantOutlineWidth = constantOutlineWidth;
            }
        }
    }

}
