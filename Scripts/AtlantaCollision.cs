using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtlantaCollision : MonoBehaviour
{
    public GameObject atlantaPrefab;
    
    private void Start()
    {
        StartCoroutine(LoadAtlantaCollision());
    }

    private IEnumerator LoadAtlantaCollision()
    {
        int i = 0;
        int buildingsPerFrame = 150;

        foreach (Transform building in atlantaPrefab.transform)
        {
            GameObject b = Instantiate(building.gameObject);
            b.transform.SetParent(transform, false);
            b.name = "Building";

            if ((++i % buildingsPerFrame) == 0)
                yield return null;
        }
    }
}