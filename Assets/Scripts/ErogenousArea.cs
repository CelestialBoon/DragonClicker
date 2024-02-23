using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ErogenousArea : MonoBehaviour
{
    private GameState gs;
    private GameData gd;
    public ErogenousType type;
    public InstrumentSO koboldInstrument;
    public OnStimulatedEvent OnStimulated = new OnStimulatedEvent();

    private void Start()
    {
        gs = GameObject.Find("/GameState").GetComponent<GameState>();
        gs.erogenousAreas[(int)type] = this;
        gd = gs.gameData;
    }

    public void StimulateWithKobold(float koboldStrength)
    {
        ClickData cd = new ClickData() { strength = koboldStrength, instrument = koboldInstrument };
        Stimulate(cd);
    }

    public void Stimulate(ClickData cd)
    {
        OnStimulated.Invoke(type, cd);
        float strength = cd.strength;
        float effectiveness = cd.instrument?.effectiveness[(int)type] ?? 1;
        if (effectiveness > 1)
        {
            //use the instrument
            strength *= effectiveness;
        }
        gd.Arouse(GetArousalIncrease(strength));
    }

    public float GetArousalIncrease(float strokeStrength)
    {
        ErogenousData ed = gd.GetErogenousData(type);
        float curveCoeff = GetCurveCoeff(ed);
        float timeCoeff = GetTimeCoeff(ed);
        float peakStr = ed.PeakStr;
        float totalStrength = strokeStrength * curveCoeff * timeCoeff * peakStr;
        Debug.Log($"Stimulated area {type} with efficiency {timeCoeff:n2}, total strength {totalStrength:n2}");
        return strokeStrength;
    }

    private float GetTimeCoeff(ErogenousData ed) 
    {
        float adjustedTimeSquared = Mathf.Pow(ed.TimeSinceLast / ed.OptimalTime, 2);
        ed.TimeSinceLast = 0f;
        return adjustedTimeSquared / (1 + adjustedTimeSquared);
    }

    private float GetCurveCoeff(ErogenousData ed)
    {
        if(ed.Curve == 0) return 1;
        float sign = ed.Curve > 0 ? 1 : -1;
        float relativeArousal = gd.arousal / gd.maxArousal;
        float powBase = Mathf.Pow(2, sign * ed.Curve);
        float res = Mathf.Pow(powBase, sign * relativeArousal);
        if (sign > 0) res /= powBase;
        return res;
    }
}
public record ErogenousData()
{
    public float Curve { get; set; } //curve = basically a modifier of how much the arousal impacts the stimulus - high positive curve means the stimulus only works on high arousal, negative curve means it works best on low arousal, 0 is uniform
    public float PeakStr { get; set; } //peakStr = maximum result achievable with one stroke
    public float OptimalTime { get; set; } //optimalTime = time between clicks for optimal result
    public float TimeSinceLast { get; set; }
    public bool HasKobold { get; set; }
    public InstrumentSO KoboldInstrument { get; set; }
    public ErogenousData(float Curve, float PeakStr, float OptimalTime, float TimeSinceLast = 0f, bool HasKobold = false, InstrumentSO KoboldInstrument = null) : this()
    {
        this.Curve = Curve;
        this.PeakStr = PeakStr;
        this.OptimalTime = OptimalTime;
        this.TimeSinceLast = TimeSinceLast;
        this.HasKobold = HasKobold;
        this.KoboldInstrument = KoboldInstrument;
    }
}
public enum ErogenousType {COCK, BALLS, ANUS, PAW, WING, HEAD, MOUTH}

public class OnStimulatedEvent : UnityEvent<ErogenousType, ClickData> { }