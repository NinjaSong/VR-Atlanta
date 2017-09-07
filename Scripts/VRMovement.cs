using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The VRMovement class handles the basics of VR user movement. It enables the user
// to scale themselves up and down by standing on their tiptoes or crouching, allowing
// them to both see Atlanta at different levels of detail and move around efficiently.
// VRMovement also has a public function MoveTo(), which moves the user around through
// an instantaneous "teleport" transition handled by this class in a coroutine.

public class VRMovement : MonoBehaviour
{
    public VRBase vr;

    public Camera cam;
    public Transform transition;
    private MeshCollider transitionCollider;
    private Material transitionMat;
        
    public float scale { get; private set; }
    public static readonly float scaleMin = 1;
    public static readonly float scaleMax = 1000;

    private Vista curVista;
    private bool transitioning;
    private Quaternion transitionRot;

    private void Awake()
    {
        scale = scaleMin;

        // Get the transition collider and material (used for the transition effect.)

        transitionCollider = transition.GetComponent<MeshCollider>();
        transitionMat = transition.GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        // If the menu is open, don't do anything.

        if (vr.menu.IsOpen())
            return;

        // Adjust rendering parameters based on the user's scale and whether they are at a vista.

        transform.localScale = Vector3.one * scale;
        cam.nearClipPlane = scale * 0.01f;
        cam.farClipPlane = Mathf.Max(5000, scale * 100f);
        QualitySettings.shadowDistance = Mathf.Max(1000, scale * 10f);

        Shader.SetGlobalVector("_CamPosition", cam.transform.position);

        float cutoff = (curVista == null ? scale * 0.5f : -100f);
        Shader.SetGlobalFloat("_CamCutoff", cutoff);

        // If the user is at too different a scale from the vista they're at, consider them no longer at it.

        if (curVista && scale > curVista.targetScale * 50)
            curVista = null;

        // Allow the user to scale themselves up and down by craning up and bending down.
        // These values are calibrated from the height control.

        float y = cam.transform.localPosition.y - VRBase.HEIGHT;
        float dPos = Mathf.Clamp01(Mathf.InverseLerp(+0.01f, VRBase.HEIGHT_DELTA_UP * 2, y)) * 4f;
        float dNeg = Mathf.Clamp01(Mathf.InverseLerp(-0.10f, VRBase.HEIGHT_DELTA_DOWN * 2, y));
        float d = 1 + (dPos - dNeg) * 0.01f;

        float prevScale = scale;
        scale = Mathf.Clamp(scale * d, scaleMin, scaleMax);
        
        // When the user scales, they usually aren't right above the scaling origin laterally,
        // which is perceived in VR as a weird sideways drift. Offset the origin position so
        // the user is always moving directly up and down.

        Vector3 delta = cam.transform.localPosition * prevScale - cam.transform.localPosition * scale;
        delta.y = 0;
        transform.position += delta;
    }

    // A public function which moves the user to the given position and scale, as long as they are not already
    // moving somewhere. The minimum and maximum scale are enforced, and calling this function without a value
    // for newScale will keep the user's scale the same.

    public void MoveTo(Vector3 newPosition, float newScale = 0, Vista newVista = null)
    {
        if (transitioning)
            return;
        if (newScale < scaleMin || newScale > scaleMax)
            newScale = scale;

        transitionRot = Quaternion.LookRotation(newPosition - transform.position);
        StartCoroutine(TransitionRoutine(newPosition, newScale, newVista));
    }

    // A public function which returns if the user is currently at the given vista.

    public bool IsAtVista(Vista vista)
    {
        return curVista == vista;
    }

    // A coroutine which transitions the user to a new position and scale (hiding the jump with a swooshy effect.)
    // As a percentage of the total time this effect takes, the user actually moves / scales 40% of the way through
    // the animation (when the transition effect is sufficiently obscuring their view), and can use the cursor again
    // when 75% of the way through the animation (when the transition effect is effectively out of the way.)

    private IEnumerator TransitionRoutine(Vector3 newPosition, float newScale, Vista newVista)
    {
        bool moved = false;
        float length = 1.5f;

        transitioning = true;
        transitionCollider.enabled = true;

        for (float f = 0; f < length; f += Time.deltaTime)
        {
            float t = f / length;
            
            transitionMat.SetFloat("_Transition", t);
            transition.rotation = transitionRot;

            if (t > 0.4 && !moved)
            {
                moved = true;

                float scaleDelta = newScale / scale;
                Vector3 delta = cam.transform.position - transform.position;
                delta.Scale(new Vector3(scaleDelta, 0, scaleDelta));

                transform.position = newPosition - delta;
                scale = newScale;
                curVista = newVista;
            }

            if (t > 0.60)
                transitionCollider.enabled = false;

            yield return null;
        }

        transitioning = false;
        transitionMat.SetFloat("_Transition", 0);
    }
}