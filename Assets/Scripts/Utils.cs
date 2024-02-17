using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public static void CopyProperties<T>(this T destination, T source)
    {
        foreach (FieldInfo fi in typeof(T).GetFields())
        {
            var value = fi.GetValue(source);
            if (value != null) fi.SetValue(destination, value);
        }
    }

}

//this is needed in order for some record niceties
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {

    }
}