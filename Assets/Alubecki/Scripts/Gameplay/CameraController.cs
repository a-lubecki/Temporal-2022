using System;
using Cinemachine;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class CameraController : MonoBehaviour {


    //min time between controls inputs => avoid bugs
    public const float DELAY_BETWEEN_MOVES_SEC = 0.2f;


    [SerializeField] CinemachineVirtualCamera vcamDolly;
    CinemachineTrackedDolly vcamTrackedDolly;
    CinemachineComposer vcamComposer;
    [SerializeField] Transform trDollyTrack;
    [SerializeField] Transform trCamLookAt;
    AudioSource audioSource;

    [SerializeField] HorizontalPosition horizontalPosition = HorizontalPosition.SW;
    [SerializeField] ZoomLevel zoomLevel = ZoomLevel.DEFAULT;

    Transform trLookAtTarget;
    HorizontalPosition lastHorizontalPosition = (HorizontalPosition)(-1);//optim for LateUpdate
    ZoomLevel lastZoomLevel = (ZoomLevel)(-1);//optim for LateUpdate
    float initialHorizontalDamping;
    float initialVerticalDamping;

    void Awake() {

        audioSource = GetComponent<AudioSource>();
        vcamTrackedDolly = vcamDolly.GetCinemachineComponent<CinemachineTrackedDolly>();
        vcamComposer = vcamDolly.GetCinemachineComponent<CinemachineComposer>();
    }

    void Start() {

        initialHorizontalDamping = vcamComposer.m_HorizontalDamping;
        initialVerticalDamping = vcamComposer.m_VerticalDamping;
    }

    void Update() {

        var c = Game.Instance.inGameControlsBehavior;

        if (c.MustRotateCameraLeft) {
            RotateLeft();
        } else if (c.MustRotateCameraRight) {
            RotateRight();
        } else if (c.MustDezoom) {
            Dezoom();
        } else if (c.MustZoom) {
            Zoom();
        }
    }

    void LateUpdate() {

        //change pivot rotation to rotate cam
        if (horizontalPosition != lastHorizontalPosition) {

            vcamTrackedDolly.m_PathPosition = GetDollyTrackPathPosition();
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
        var newPosition = (trLookAtTarget != null) ? trLookAtTarget.transform.position : Vector3.zero;

        if (newPosition != trCamLookAt.transform.position) {

            vcamComposer.m_HorizontalDamping = initialHorizontalDamping;
            vcamComposer.m_VerticalDamping = initialVerticalDamping;

            trCamLookAt.transform.position = newPosition;
        }
    }

    public void SetLookAtTarget(Transform trLookAtTarget) {
        this.trLookAtTarget = trLookAtTarget;
    }

    public void ResetRotationAndZoom() {

        horizontalPosition = HorizontalPosition.SW;
        zoomLevel = ZoomLevel.DEFAULT;
    }

    void PrepareCamToMove() {

        Game.Instance.inGameControlsBehavior.DisableControlsForSeconds(DELAY_BETWEEN_MOVES_SEC);

        //avoid a camera bug when damping is not zero
        vcamComposer.m_HorizontalDamping = 0;
        vcamComposer.m_VerticalDamping = 0;
    }

    void RotateLeft() {

        PrepareCamToMove();

        horizontalPosition = horizontalPosition switch {
            HorizontalPosition.SW => HorizontalPosition.NW,
            HorizontalPosition.NW => HorizontalPosition.NE,
            HorizontalPosition.NE => HorizontalPosition.SE,
            HorizontalPosition.SE => HorizontalPosition.SW,
            _ => throw new NotImplementedException()
        };
    }

    void RotateRight() {

        PrepareCamToMove();

        horizontalPosition = horizontalPosition switch {
            HorizontalPosition.SW => HorizontalPosition.SE,
            HorizontalPosition.NW => HorizontalPosition.SW,
            HorizontalPosition.NE => HorizontalPosition.NW,
            HorizontalPosition.SE => HorizontalPosition.NE,
            _ => throw new NotImplementedException()
        };
    }

    void Dezoom() {

        PrepareCamToMove();

        zoomLevel = zoomLevel switch {
            ZoomLevel.CLOSE => ZoomLevel.DEFAULT,
            ZoomLevel.DEFAULT or ZoomLevel.FAR => ZoomLevel.FAR,
            _ => throw new NotImplementedException()
        };
    }

    void Zoom() {

        PrepareCamToMove();

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

        ZoomLevel.CLOSE => 25,
        ZoomLevel.DEFAULT => 50,
        ZoomLevel.FAR => 80,
        _ => throw new NotImplementedException()
    };

    public float GetDollyTrackSize() => zoomLevel switch {

        ZoomLevel.CLOSE => 0.6f,
        ZoomLevel.DEFAULT => 0.8f,
        ZoomLevel.FAR => 0.5f,
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
