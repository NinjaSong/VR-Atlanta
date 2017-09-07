using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tweet : Selectable, IPointerEnterHandler, IPointerExitHandler
{
    public Color color;
    public RectTransform panelRect;
    public Image panelImage;
    public RectTransform logoRect;
    public Image logoImage;
    public RectTransform contentRect;
    public CanvasGroup contentGroup;

    private RectTransform rect;
    private OVRRaycaster raycaster;
    private Transform target;

    private bool open;
    private float openAmount;
    private float openSpeed = 0.035f;

    private float height = 0;
    private float heightMin = 100;
    private float heightFactor;
    private float heightDrag;

    protected override void Awake()
    {
        base.Awake();
        if (!Application.isPlaying)
            return;
        
        GetComponent<Canvas>().worldCamera = FindObjectOfType<VRMovement>().cam;
        raycaster = GetComponent<OVRRaycaster>();
        raycaster.pointer = FindObjectOfType<OVRGazePointer>().gameObject;
        
        rect = GetComponent<RectTransform>();
        target = Camera.main.transform;

        heightFactor = Random.Range(0.5f, 0.9f);
        heightDrag = Random.Range(0.02f, 0.07f);
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        // Animate the tweet opening and closing

        float dt = Time.deltaTime * 60;
        openAmount = Mathf.MoveTowards(openAmount, open ? 1 : 0, openSpeed * dt);
        float t = Util.Smootherstep(openAmount);

        float w = Mathf.Lerp(100, contentRect.sizeDelta.x, t);
        float h = Mathf.Lerp(100, contentRect.sizeDelta.y, t);
        panelRect.sizeDelta = new Vector2(w, h);
        panelImage.color = Color.Lerp(color, Color.white, t);

        rect.sizeDelta = panelRect.sizeDelta;

        float logoX = Mathf.Lerp(-50, -70, t);
        float logoY = Mathf.Lerp(-50, -55, t);
        logoRect.anchoredPosition = new Vector2(logoX, logoY);
        logoRect.sizeDelta = Vector2.one * Mathf.Lerp(80, 60, t);
        logoImage.color = Color.Lerp(Color.white, color, t);

        contentGroup.alpha = t;
        contentGroup.interactable = (openAmount == 1);
        contentGroup.blocksRaycasts = (openAmount == 1);

        // Make the tweet float to the user's height * this tweet's height factor

        height = Mathf.Lerp(height, target.position.y * heightFactor, heightDrag);
        height = Mathf.Max(height, heightMin);
        Vector3 p = transform.position;
        p.y = height;
        transform.position = p;

        // Make the tweet look at the user

        Vector3 delta = target.position - transform.position;
        transform.LookAt(transform.position - delta);

        // Bias the tweet's scale towards an "isoscale" (keeping the tweets the same size
        // regardless of their distance from the user's eyes), doing so especially strongly
        // when the tweet is open. This keeps the text at a comfortably readable size.

        float realScale = 0.15f;
        float isoScale = 0.00125f * delta.magnitude;
        float scale = Mathf.Lerp(realScale, isoScale, 0.2f + t * 0.6f);
        transform.localScale = Vector3.one * scale;

        // Sort the tweet in the raycaster by its distance to the user's eyes.

        raycaster.sortOrder = (int) -delta.magnitude;
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