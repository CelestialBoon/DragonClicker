using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Utils
{
    public static int RoundP(float f) { //as in, RoundProbabilistically
        int res = Mathf.FloorToInt(f);
        float diff = f - res;
        if(Random.Range(0f, 1f) <= diff)
        {
            res += 1;
        }
        return res;
    }

    public static float MaxAbs(float a, float b)
    {
        if (Mathf.Abs(a) > Mathf.Abs(b)) return a; else return b;
    }

    public static void CopyProperties<T>(this T destination, T source)
    {
        foreach (FieldInfo fi in typeof(T).GetFields())
        {
            var value = fi.GetValue(source);
            if (value != null) fi.SetValue(destination, value);
        }
    }

    public static void PutAt<T>(this List<T> list, T element, int index)
    {
        if (list.Count > index) list[index] = element;
        else {
            for(int i = list.Count; i<index; i++)
            {
                list.Add(default);
            }
            list.Add(element);
        }
    }
    public static void Put<T,U>(this Dictionary<T,U> dict, T key, U value)
    {
        if (dict.ContainsKey(key)) dict[key] = value;
        else dict.Add(key, value);
    }
}

//this is needed in order for some record niceties
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {

    }
}