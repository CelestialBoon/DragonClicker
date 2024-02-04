using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArousalMeterValue : MonoBehaviour
{
    public double arousalMeter;
    private const double maxLimit = 100.0;
    private const double minLimit = 0.0;

    // Getting the arousal meter
    public double getArousalMeter()
    {
        return arousalMeter;
    }

    // Setting the arousal meter
    public void setArousalMeter(double value)
    {
        arousalMeter = value;
        limitingArousalMeter();
    }

    // Increasing the arousal meter
    public void addArousalMeter(double value)
    {
        arousalMeter += value;
        limitingArousalMeter();
    }

    // Subtracting the arousal meter
    public void subtractArousalMeter(double value)
    {
        arousalMeter -= value;
        limitingArousalMeter();
    }

    // Prevent value from going over 100 or below 0
    private void limitingArousalMeter()
    {
        if (arousalMeter > maxLimit)
        {
            arousalMeter = maxLimit;
        }
        if (arousalMeter < minLimit)
        {
            arousalMeter = minLimit;
        }
    }
}
