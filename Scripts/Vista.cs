using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A Vista is a clickable icon that transports the user to a predetermined location and scale.
// This is great for creating easily-accessible scenic positions (on top of buildings, etc.)

public class Vista : Selectable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 targetPosition;
    public float targetScale = 1;

    public Color color;
    public RectTransform panelRect;
    public Image panelImage;
    public RectTransform logoRect;
    public Image logoImage;
    public RectTransform contentRect;
    public CanvasGroup contentGroup;

    private VRMovement user;
    private RectTransform rect;
    private OVRRaycaster raycaster;
    private Transform target;

    private bool open;
    private float openAmount;
    private float openSpeed = 0.035f;

    protected override void Awake()
    {
        base.Awake();
        if (!Application.isPlaying)
            return;

        GetComponent<Canvas>().worldCamera = FindObjectOfType<VRMovement>().cam;
        user = FindObjectOfType<VRMovement>();
        rect = GetComponent<RectTransform>();
        raycaster = GetComponent<OVRRaycaster>();
        raycaster.pointer = FindObjectOfType<OVRGazePointer>().gameObject;
        target = Camera.main.transform;
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        // Deactivate or reactivate this vista if the user is currently at it

        panelRect.gameObject.SetActive(!user.IsAtVista(this));

        // Animate the panel opening and closing

        float dt = Time.deltaTime * 60;
        openAmount = Mathf.MoveTowards(openAmount, open ? 1 : 0, openSpeed * dt);
        float t = Util.Smootherstep(openAmount);

        float w = Mathf.Lerp(100, contentRect.sizeDelta.x, t);
        float h = Mathf.Lerp(100, contentRect.sizeDelta.y, t);
        panelRect.sizeDelta = new Vector2(w, h);
        panelImage.color = Color.Lerp(color, Color.white, t);

        rect.sizeDelta = panelRect.sizeDelta;
        
        float logoY = Mathf.Lerp(-50, -45, t);
        logoRect.anchoredPosition = new Vector2(0, logoY);
        logoRect.sizeDelta = Vector2.one * Mathf.Lerp(80, 60, t);
        logoImage.color = Color.Lerp(Color.white, color, t);

        contentGroup.alpha = t;
        contentGroup.interactable = (openAmount == 1);
        contentGroup.blocksRaycasts = (openAmount == 1);

        // Make the panel look at the user

        Vector3 delta = target.position - transform.position;
        transform.LookAt(transform.position - delta);

        // Bias the panel's scale towards an "isoscale" (keeping the panels the same size
        // regardless of their distance from the user's eyes), doing so especially strongly
        // when the panel is open. This keeps the text at a comfortably readable size.

        float realScale = 0.15f;
        float isoScale = 0.00125f * delta.magnitude;
        float scale = Mathf.Lerp(realScale, isoScale, 0.2f + t * 0.6f);
        transform.localScale = Vector3.one * scale;

        // Sort the panel in the raycaster by its distance to the user's eyes.

        raycaster.sortOrder = (int) -delta.magnitude;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        FindObjectOfType<VRMovement>().MoveTo(targetPosition, targetScale, this);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        open = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        open = false;
    }
}