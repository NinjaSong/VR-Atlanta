using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHighlighter : MonoBehaviour
{
    public Material highlightMat;

    private GameObject active;
    private MeshRenderer render;

    public void Highlight(GameObject g)
    {
        if (active != g)
        {
            active = g;
            if (render)
                Destroy(render);
            render = active.AddComponent<MeshRenderer>();
            render.material = highlightMat;
        }
    }

    public void Off()
    {
        active = null;
        if (render)
            Destroy(render);
    }
}