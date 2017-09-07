using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VRHeightCalibrationPanel : VRMenuPanel
{
    public VRBase vr;
    public Text text;
    public Transform user;
    public Button button;
    private VRHeightCalibrationState state;

    private float bPosTarg;
    private float bPos;
    private float bAngleTarg;
    private float bAngle;
    private float bDrag = 0.125f;

    private new void Update()
    {
        base.Update();

        if (state == VRHeightCalibrationState.Height)
            bPosTarg = (user.localPosition.y - 0.05f - VRBase.HEIGHT) * 500;

        float dt = Time.deltaTime * 60;
        bPos = Mathf.Lerp(bPos, bPosTarg, bDrag * dt);
        bAngle = Mathf.Lerp(bAngle, bAngleTarg, bDrag * dt);

        Vector3 p = transform.localPosition;
        p.y = bPos;
        button.transform.localPosition = p;
        button.transform.localRotation = Quaternion.Euler(bAngle, 0, 0);
    }

    public void StartCalibration()
    {
        SetState(VRHeightCalibrationState.Height);
        bPos = bPosTarg;
        bAngle = bAngleTarg;
    }

    // Callback from the calibration button.

    public void Advance()
    {
        switch (state)
        {
            case VRHeightCalibrationState.Height:
                VRBase.HEIGHT = user.localPosition.y;
                SetState(VRHeightCalibrationState.DeltaUp);
                break;

            case VRHeightCalibrationState.DeltaUp:
                VRBase.HEIGHT_DELTA_UP = user.localPosition.y - VRBase.HEIGHT;
                SetState(VRHeightCalibrationState.DeltaDown);
                break;

            case VRHeightCalibrationState.DeltaDown:
                VRBase.HEIGHT_DELTA_DOWN = user.localPosition.y - VRBase.HEIGHT;
                vr.menu.CalibrationComplete();
                break;
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    // Helper function that sets the current state of the height calibrator. This sets the text as well as where the button is.

    private void SetState(VRHeightCalibrationState state)
    {
        this.state = state;

        if (state == VRHeightCalibrationState.Height)
        {
            text.text = "Please stand up straight\nand look forward at this message.\n\nThen, press the button.";
            bAngleTarg = 0;
        }

        else if (state == VRHeightCalibrationState.DeltaUp)
        {
            text.text = "Hold your chin up\nand stand on your toes.\n\nThen, press the button.";
            bPosTarg = 400f;
            bAngleTarg = -35;
        }

        else // DeltaDown
        {
            text.text = "Now, bend your knees slightly\n and hold your head down.\n\nThen, press the button.";
            bPosTarg = -400f;
            bAngleTarg = 30;
        }
    }
}

public enum VRHeightCalibrationState
{
    Height, DeltaUp, DeltaDown
}