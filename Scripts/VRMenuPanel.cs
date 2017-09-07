using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMenuPanel : MonoBehaviour
{
    private RectTransform rect;
    private CanvasGroup group;

    private bool open;
    private float openAmount;
    private float openAccel = 0.2f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
    }

    public void Update()
    {
        float dt = Time.deltaTime * 60;
        openAmount = Mathf.Lerp(openAmount, open ? 1 : 0, dt * openAccel);

        rect.localScale = new Vector3(openAmount, openAmount, 1);
        group.blocksRaycasts = open;
        group.interactable = open;
        group.alpha = openAmount;
    }

    public void Open()
    {
        open = true;
    }

    public void Close()
    {
        open = false;
    }

    public bool IsOpen()
    {
        return open;
    }
}