using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The VRNavigation class allows the user to click on the 'ground' of Atlanta,
// moving them to the location where they clicked. It's actually just a big UI canvas
// on the floor so that it interacts properly with the other UI elements in the scene.
// For fun, it also places a nice-looking cursor where the user is looking.

public class VRNavigation : Selectable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public VRBase vr;
    public Transform gazePointer;
    public Transform cursor;
    public Light cursorLight;

    private float cursorScale;

    private bool active;
    private float activeSize;
    private float activeDrag = 0.25f;

    private void LateUpdate()
    {
        // If we're in the editor or the menu is open, don't do anything.

        if (!Application.isPlaying)
            return;
        if (vr.menu.IsOpen())
            active = false;

        // Update the position, scale, and appearance of the cursor

        if (active)
        {
            cursor.position = gazePointer.position;
            float newHeight = vr.movement.transition.position.y - cursor.transform.position.y;
            cursorScale = newHeight / VRBase.HEIGHT;
            cursorScale = Mathf.Clamp(cursorScale, VRMovement.scaleMin, VRMovement.scaleMax);
        }

        float activeTarg = active ? 1 : 0;
        activeSize = Mathf.Lerp(activeSize, activeTarg, activeDrag);
        cursor.localScale = Vector3.one * cursorScale * activeSize;
        cursorLight.intensity = activeSize * 2;
        cursorLight.range = vr.movement.scale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        vr.movement.MoveTo(eventData.pointerCurrentRaycast.worldPosition, cursorScale);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        active = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        active = false;
    }
}