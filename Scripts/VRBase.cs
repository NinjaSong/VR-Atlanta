using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRBase : MonoBehaviour
{
    public VRMovement movement;
    public VRNavigation navigation;
    public VRMenu menu;
    
    public static float HEIGHT = 1.5f;
    public static float HEIGHT_DELTA_UP = 0f;
    public static float HEIGHT_DELTA_DOWN = 0f;
}