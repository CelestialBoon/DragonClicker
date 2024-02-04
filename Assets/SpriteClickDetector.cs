using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteClickDetector : MonoBehaviour
{
    public ArousalMeterValue arousalMeterValue;
    private Animation anim;
    private bool strokingUp;
    public string strokeUpAnimation;
    public string strokeDownAnimation;
    private void OnMouseDown()
    {
        // Increase arousal Meter - DONE
        arousalMeterValue.addArousalMeter(0.01);
        Debug.Log(arousalMeterValue.getArousalMeter());

        // Trigger animation playing - TO DO
        if (strokingUp) anim.Play(strokeUpAnimation);
        else anim.Play(strokeDownAnimation);
    }
}
