using UnityEngine;

public static class Util
{
    public static float Smootherstep(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * t * (t * (t * 6 - 15) + 10);
    }
}