using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ErogenousArea : MonoBehaviour
{
    [SerializeField] private GameState gs;
    public ErogenousType type;
    public OnStimulatedEvent OnStimulated = new OnStimulatedEvent();

    private void OnMouseDown()
    {
        Debug.Log("clicked");
        ClickData cd = gs.GetManualClickData(type);
        gs.Arouse(GetArousalIncrease(cd));
    }

    public float GetArousalIncrease(ClickData clickData)
    {
        ErogenousData ed = gs.GetErogenousData(type);
        OnStimulated.Invoke(type, clickData);
        float strokeStrength = clickData.parameters.baseStrength * clickData.multiplier;
        Debug.Log($"StrokeStr: {strokeStrength}");
        float curveCoeff = GetCurveCoeff(ed);
        float timeCoeff = GetTimeCoeff(ed);
        float peakStr = ed.PeakStr;
        Debug.Log($"CurveCoeff: {curveCoeff}");
        Debug.Log($"TimeCoeff: {timeCoeff}");
        Debug.Log($"PeakStr: {peakStr}");
        return strokeStrength * curveCoeff * timeCoeff * peakStr;
    }

    private float GetTimeCoeff(ErogenousData ed) 
    {
        float adjustedTimeSquared = Mathf.Pow(ed.TimeSinceLast * ed.OptimalFreq, 2);
        ed.TimeSinceLast = 0f;
        return adjustedTimeSquared / (1 + adjustedTimeSquared);
    }

    private float GetCurveCoeff(ErogenousData ed)
    {
        if(ed.Curve == 0) return 1;
        float sign = ed.Curve > 0 ? 1 : -1;
        float relativeArousal = gs.arousal / gs.maxArousal;
        float powBase = Mathf.Pow(2, sign * ed.Curve);
        float res = Mathf.Pow(powBase, sign * relativeArousal);
        if (sign > 0) res /= powBase;
        return res;
    }
}
public record ErogenousData()
{
    public float Curve { get; set;}
    public float PeakStr { get; set; }
    public float OptimalFreq { get; set; }
    public float TimeSinceLast { get; set; }
    public ErogenousData(float Curve, float PeakStr, float OptimalFreq, float TimeSinceLast = 0f) : this()
    {
        this.Curve = Curve;
        this.PeakStr = PeakStr;
        this.OptimalFreq = OptimalFreq;
        this.TimeSinceLast = TimeSinceLast;
    }
}
//curve = basically a modifier of how much the arousal impacts the stimulus - high positive curve means the stimulus only works on high arousal, negative curve means it works best on low arousal, 0 is uniform
//peakStr = maximum result achievable with one stroke
//optimalFreq = clicks a second for optimal result
public enum ErogenousType {COCK, BALLS, ANUS, PAW, BELLY, HEAD, MOUTH, OTHER}

public class OnStimulatedEvent : UnityEvent<ErogenousType, ClickData> { }