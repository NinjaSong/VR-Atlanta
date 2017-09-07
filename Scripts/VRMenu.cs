using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRMenu : MonoBehaviour
{
    public Color menuColorClosed;
    public Color menuColorOpen;

    [Space(10)]
    [Header("Internal References")]
    public VRBase vr;
    public Transform rootMask;
    public MeshRenderer rootRenderer;
    public MeshCollider rootCollider;
    public RectTransform rootCanvas;
    public VRMenuPanel mainPanel;
    public VRHeightCalibrationPanel heightCalibrationPanel;

    private VRMenuPanel activePanel;

    private bool busy;
    private bool open = true;
    private float openAmount = 1f;
    private float openSpeed = 0.1f;

    private float angle;
    private float angleTarg;
    private float angleDrag = 0.1f;

    private void Awake()
    {
        ChangePanel(heightCalibrationPanel);
    }

    private void Update()
    {
        // Animate the menu opening and closing

        float dt = Time.deltaTime * 60;
        openAmount = Mathf.MoveTowards(openAmount, open ? 1 : 0, openSpeed * dt);
        float t = Util.Smootherstep(openAmount);

        Color c = Color.Lerp(menuColorClosed, menuColorOpen, t);
        rootRenderer.material.SetColor("_Color", c);
        rootCollider.enabled = openAmount > 0;

        // Let the user open the menu

        if (OVRInput.GetDown(OVRInput.Button.Two))
            Open();

        // Make the menu always track the camera.
        // First, set the origin of the menu to the ground under the user's eyes.

        Vector3 root = vr.movement.cam.transform.localPosition;
        root.y = 0;
        transform.position = vr.movement.transform.TransformPoint(root);

        // Rotate the menu to be in front of where the user is looking.

        Vector3 look = vr.movement.cam.transform.forward;
        look.y = 0;
        angleTarg = Quaternion.LookRotation(look, Vector3.up).eulerAngles.y;
        angle = Mathf.LerpAngle(angle, angleTarg, angleDrag);
        transform.rotation = Quaternion.Euler(0, angle, 0);

        // Scale the menu with the user.

        transform.localScale = Vector3.one * vr.movement.scale;

        // Place the root mask (the fade) around the user's eyes.

        rootMask.position = vr.movement.cam.transform.position;

        // Place the canvas at the user's height.

        rootCanvas.localPosition = new Vector3(0, VRBase.HEIGHT, 1.25f);
    }

    private void Open()
    {
        if (!open && !busy)
        {
            open = true;
            ChangePanel(mainPanel);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void Close()
    {
        if (open && !busy)
        {
            open = false;
            ChangePanel(null);
        }
    }

    private void ChangePanel(VRMenuPanel newPanel)
    {
        if (activePanel == newPanel)
            return;
        StartCoroutine(ChangePanelRoutine(newPanel));
    }

    private IEnumerator ChangePanelRoutine(VRMenuPanel newPanel)
    {
        busy = true;

        if (activePanel)
        {
            activePanel.Close();
            yield return new WaitForSeconds(0.3f);
        }
        
        activePanel = newPanel;
        if (activePanel)
            activePanel.Open();

        busy = false;
    }

    public bool IsOpen()
    {
        return open;
    }

    // ======================================================================================================================== BUTTON CALLBACKS

    // Called by the begin button.

    public void Begin()
    {
        Close();
    }

    // Called by the height calibration button.

    public void RecalibrateHeight()
    {
        ChangePanel(heightCalibrationPanel);
        heightCalibrationPanel.StartCalibration();
    }

    // Called by the height calibration script when the user completes calibration.

    public void CalibrationComplete()
    {
        ChangePanel(mainPanel);
    }
}