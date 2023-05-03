using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyMath 
{
    public static float Hypothenuse(float a, float b){
        return Mathf.Sqrt(Mathf.Pow(a,2) + Mathf.Pow(b,2));
    }
}
