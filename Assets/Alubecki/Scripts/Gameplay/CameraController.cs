using System;
using Cinemachine;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class CameraController : MonoBehaviour {


    //min time between controls inputs => avoid bugs
    public const float DELAY_BETWEEN_MOVES_SEC = 0.2f;


    [SerializeField] CinemachineVirtualCamera vcamDolly;
    [SerializeField] Transform trDollyTrack;
    [SerializeField] Transform trCamLookAt;
    AudioSource audioSource;

    [SerializeField] HorizontalPosition horizontalPosition = HorizontalPosition.SW;
    [SerializeField] ZoomLevel zoomLevel = ZoomLevel.DEFAULT;

    HorizontalPosition lastHorizontalPosition = (HorizontalPosition)(-1);//optim for LateUpdate
    ZoomLevel lastZoomLevel = (ZoomLevel)(-1);//optim for LateUpdate


    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {

        var c = Game.Instance.inGameControlsBehavior;

        if (c.MustRotateCameraLeft) {
            rotateLeft();
        } else if (c.MustRotateCameraRight) {
            rotateRight();
        } else if (c.MustDezoom) {
            dezoom();
        } else if (c.MustZoom) {
            zoom();
        }
    }

    void LateUpdate() {

        //change pivot rotation to rotate cam
        if (horizontalPosition != lastHorizontalPosition) {

            vcamDolly.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition = GetDollyTrackPathPosition();
            lastHorizontalPosition = horizontalPosition;

            audioSource.Play();
        }

        //change slider position to zoom/dezoom
        if (zoomLevel != lastZoomLevel) {

            trDollyTrack.localPosition = new Vector3(0, GetDollyTrackYPosition(), 0);
            trDollyTrack.localScale = Vector3.one * GetDollyTrackSize();
            lastZoomLevel = zoomLevel;

            audioSource.Play();
        }

        //change "look at" position to change orientation to selected object
        var selectedElem = Game.Instance.elementsSelectionBehavior.SelectedElement;
        trCamLookAt.transform.position = selectedElem?.transform.position ?? Vector3.zero;
    }

    void rotateLeft() {

        Game.Instance.inGameControlsBehavior.DisableControlsForSeconds(DELAY_BETWEEN_MOVES_SEC);

        horizontalPosition = horizontalPosition switch {
            HorizontalPosition.SW => HorizontalPosition.NW,
            HorizontalPosition.NW => HorizontalPosition.NE,
            HorizontalPosition.NE => HorizontalPosition.SE,
            HorizontalPosition.SE => HorizontalPosition.SW,
            _ => throw new NotImplementedException()
        };
    }

    void rotateRight() {

        Game.Instance.inGameControlsBehavior.DisableControlsForSeconds(DELAY_BETWEEN_MOVES_SEC);

        horizontalPosition = horizontalPosition switch {
            HorizontalPosition.SW => HorizontalPosition.SE,
            HorizontalPosition.NW => HorizontalPosition.SW,
            HorizontalPosition.NE => HorizontalPosition.NW,
            HorizontalPosition.SE => HorizontalPosition.NE,
            _ => throw new NotImplementedException()
        };
    }

    void dezoom() {

        Game.Instance.inGameControlsBehavior.DisableControlsForSeconds(DELAY_BETWEEN_MOVES_SEC);

        zoomLevel = zoomLevel switch {
            ZoomLevel.CLOSE => ZoomLevel.DEFAULT,
            ZoomLevel.DEFAULT or ZoomLevel.FAR => ZoomLevel.FAR,
            _ => throw new NotImplementedException()
        };
    }

    void zoom() {

        Game.Instance.inGameControlsBehavior.DisableControlsForSeconds(DELAY_BETWEEN_MOVES_SEC);

        zoomLevel = zoomLevel switch {
            ZoomLevel.CLOSE or ZoomLevel.DEFAULT => ZoomLevel.CLOSE,
            ZoomLevel.FAR => ZoomLevel.DEFAULT,
            _ => throw new NotImplementedException()
        };
    }

    public float GetDollyTrackPathPosition() => horizontalPosition switch {

        HorizontalPosition.SW => 1.5f,
        HorizontalPosition.NW => 0.5f,
        HorizontalPosition.NE => 3.5f,
        HorizontalPosition.SE => 2.5f,
        _ => throw new NotImplementedException()
    };

    public float GetDollyTrackYPosition() => zoomLevel switch {

        ZoomLevel.CLOSE => 20,
        ZoomLevel.DEFAULT => 40,
        ZoomLevel.FAR => 80,
        _ => throw new NotImplementedException()
    };

    public float GetDollyTrackSize() => zoomLevel switch {

        ZoomLevel.CLOSE => 0.6f,
        ZoomLevel.DEFAULT => 1,
        ZoomLevel.FAR => 0.6f,
        _ => throw new NotImplementedException()
    };

    public float GetOutlinesThicknessMultiplier() => zoomLevel switch {

        ZoomLevel.CLOSE => 2,
        ZoomLevel.DEFAULT => 1,
        ZoomLevel.FAR => 0.6f,
        _ => throw new NotImplementedException()
    };

}

public enum HorizontalPosition : int { //North, South, East, West
    SW,
    NW,
    NE,
    SE,
}

public enum ZoomLevel {
    CLOSE,
    DEFAULT,
    FAR,
}
