using System;
using System.Collections;
using UnityEngine;

//this class holds the mechanical game data that gets saved and loaded
//as well as the mechanics to update it
public class GameData
{
    public ErogenousData[] erogenousDatas;

    public float clickStrength;
    public float koboldStrength;

    public float arousal;
    public float maxArousal;
    public float decayArousal;

    public int fluid;

    public float buildup;
    public float ratioBuildup;
    public float refractoryBuildup;
    public float maxBuildup;

    public float bucket;
    public float maxBucket;
    public BucketState bucketState;

    public float refractoryTime;
    public float maxRefractoryTime;
    public float refractorySpeed;

    public float orgasmTime;
    public float maxOrgasmTime;

    public int gold;
    public float goldMultiplier;

    public GameState gs;

    public GameData Initialize(GameState gameState)
    {
        gs = gameState;

        clickStrength = 1;
        koboldStrength = 0.3f;

        maxArousal = 100;
        decayArousal = 3;

        ratioBuildup = 0.1f;
        refractoryBuildup = 20f;
        maxBuildup = 100;

        maxBucket = 70;

        maxRefractoryTime = 4;
        maxOrgasmTime = 3;

        goldMultiplier = 1.05f;

        erogenousDatas = new ErogenousData[Enum.GetNames(typeof(ErogenousType)).Length]; //TODO compile full list of zones
        erogenousDatas[(int)ErogenousType.COCK] = new ErogenousData(2, 5, 5f);
        erogenousDatas[(int)ErogenousType.MOUTH] = new ErogenousData(-1f, 7, 2f);

        arousal = 0;
        fluid = 0;
        buildup = 0;
        bucket = 0;
        refractoryTime = 0;
        orgasmTime = 0;
        gold = 0;

        bucketState = BucketState.Empty;
        return this;
    }

    public void Start_()
    {
        if (bucket >= maxBucket)
            UpdateBucket(BucketState.Full);
        else UpdateBucket(BucketState.Empty);
    }

    public void FixedUpdate_() //formulas to later tune For His Pleasure
    {
        if (orgasmTime > 0 || refractoryTime > 0) return;

        buildup = Mathf.Min(maxBuildup, buildup + arousal * ratioBuildup * Mathf.Pow(arousal / maxArousal, 2) * Time.fixedDeltaTime);

        arousal = Mathf.Max(0, arousal - decayArousal * Time.fixedDeltaTime);

        //TODO add timeSinceLast to all the erogenous zones
        int len = Enum.GetNames(typeof(ErogenousType)).Length;
        for (int i = 0; i < len; i++)
        {
            if (erogenousDatas[i] != null)
            {
                erogenousDatas[i].TimeSinceLast += Time.fixedDeltaTime;
            }
        }
    }

    public void Arouse(float amount)
    {
        if (orgasmTime > 0) return;

        if (buildup >= maxBuildup) amount *= 5;
        if (refractoryTime > 0) amount *= 0.2f;

        arousal += amount;
        gs.OnAroused.Invoke(amount);

        if (arousal > maxArousal) gs.OnOrgasm.Invoke();
    }

    internal void EmptyBucket()
    {
        fluid += Mathf.RoundToInt(bucket);
        bucket = 0;
        UpdateBucket(BucketState.None);
    }
    internal void UpdateBucket(BucketState state)
    {
        if (bucketState != state)
        {
            bucketState = state;
            gs.OnBucketStateChanged.Invoke(state);
        }
    }

    internal IEnumerator Orgasm()
    {
        var halfOrgasmTime = maxOrgasmTime * 0.5f;

        refractorySpeed = maxArousal / maxRefractoryTime;

        //in the first half we fill the bucket
        float emptyingSpeed = maxBuildup / halfOrgasmTime;
        float emptyingPerTick = emptyingSpeed * Time.fixedDeltaTime;
        for (orgasmTime = halfOrgasmTime; orgasmTime > 0; orgasmTime -= Time.fixedDeltaTime)
        {
            if (buildup > 0)
            {
                buildup -= emptyingPerTick;
                if (bucket < maxBucket)
                {
                    bucket = Mathf.Min(maxBucket, bucket + emptyingPerTick);
                    if (bucket == maxBucket)
                    {
                        UpdateBucket(BucketState.Full);
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }
        buildup = 0;
        //in the second time the arousal starts decreasing
        for (orgasmTime = halfOrgasmTime; orgasmTime > 0; orgasmTime -= Time.fixedDeltaTime)
        {
            arousal -= refractorySpeed * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        orgasmTime = 0;
        gs.OnRefractory.Invoke();
    }

    internal IEnumerator RefractoryPeriod()
    {
        float buildupSpeed = refractoryBuildup / maxRefractoryTime;
        for (refractoryTime = maxRefractoryTime; refractoryTime > 0; refractoryTime -= Time.fixedDeltaTime)
        {
            buildup += buildupSpeed * Time.fixedDeltaTime;
            if (arousal > 0)
            {
                arousal -= refractorySpeed * Time.fixedDeltaTime;
            } 
            yield return new WaitForFixedUpdate();
        }
        refractorySpeed = 0;
    }

    internal void sellCum()
    {
        gold += Mathf.RoundToInt(fluid * goldMultiplier);
        fluid = 0;
    }

    internal ErogenousData GetErogenousData(ErogenousType type)
    {
        return erogenousDatas[(int)type];
    }
}
