using UnityEngine;


/// <summary>
/// Subclass used to take camera distance from level into account.
/// Originally, when the camera is far, the outline seems bigger than when the camera is close.
/// Use constantOutlineWidth instead of outlineWidth.
/// </summary>
public class ZoomableOutline : Outline {


    public static bool areZoomableOutlinesEnabled = true;


    CameraController cameraController;
    [SerializeField] float constantOutlineWidth = 1;

    private float lastThicknessMultiplier = -1;
    private float lastConstantOutlineWidth = -1;


    void LateUpdate() {

        //call GetComponent in LateUpdate because Outline class doesn't have protected Awake method
        if (cameraController == null) {
            cameraController = Camera.main.GetComponent<CameraController>();
        }

        if (cameraController == null) {
            return;
        }

        var multiplier = areZoomableOutlinesEnabled ? cameraController.GetOutlinesThicknessMultiplier() : 0;
        if (multiplier != lastThicknessMultiplier || constantOutlineWidth != lastConstantOutlineWidth) {

            //change original outline width (parent attribute)
            OutlineWidth = constantOutlineWidth * multiplier;

            lastThicknessMultiplier = multiplier;
            lastConstantOutlineWidth = constantOutlineWidth;
        }
    }

}
