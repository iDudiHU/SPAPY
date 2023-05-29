
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers {
    public static float EaseOutCirc(float x)
    {
        return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
    }
}
