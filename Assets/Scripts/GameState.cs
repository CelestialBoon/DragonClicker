using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public const float tick = 0.02f;

    public float arousal;
    public float maxArousal;
    public float decayArousal;

    public int fluid;

    public float buildup;
    public float ratioBuildup;
    public float maxBuildup;

    public float refractoryTime;
    public float maxRefractoryTime;
    public float refractorySpeed;

    public float orgasmTime;
    public float maxOrgasmTime;

    private void Awake()
    {
        LoadGameValues();
    }

    private void LoadGameValues() //later this will have to implement savestates
    {
        maxArousal = 70;
        decayArousal = 2;

        ratioBuildup = 0.2f;
        maxBuildup = 100;
        
        maxRefractoryTime = 10;
        maxOrgasmTime = 3;

        arousal = 0;
        fluid = 0;
        buildup = 0;
        refractoryTime = 0;
        orgasmTime = 0;
    }

    private void FixedUpdate()
    {
        if (orgasmTime > 0 || refractoryTime > 0) return;

        buildup = Mathf.Min(maxBuildup, buildup + arousal * ratioBuildup * Mathf.Pow(arousal / maxArousal, 3) * Time.fixedDeltaTime);

        arousal = Mathf.Max(0, arousal - decayArousal * Time.fixedDeltaTime);
    }

    internal IEnumerator Orgasm()
    {
        var halfOrgasmTime = maxOrgasmTime * 0.5f;
        
        fluid += (int)buildup;
        buildup = 0;
        refractorySpeed = maxArousal / maxRefractoryTime;
        orgasmTime = halfOrgasmTime;
        yield return new WaitForSeconds(halfOrgasmTime);
        for(; orgasmTime > 0; orgasmTime -= tick)
        {
            arousal -= refractorySpeed * tick;
            yield return new WaitForSeconds(tick);
        }
        StartCoroutine(RefractoryPeriod());
    }

    internal IEnumerator RefractoryPeriod()
    {
        for(refractoryTime = maxRefractoryTime; refractoryTime > 0; refractoryTime -= tick)
        {
            if(arousal > 0)
            {
                arousal -= refractorySpeed * tick;
            }
            yield return new WaitForSeconds(tick);
        }
        refractorySpeed = 0;
    }
}
