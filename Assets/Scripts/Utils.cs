using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static int RoundP(float f) { //as in, RoundProbabilistically
        int res = (int)f;
        float diff = f - res;
        if(Random.Range(0, 1) <= diff)
        {
            res += 1;
        }
        return res;
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {

    }
}