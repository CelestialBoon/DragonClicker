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

    public float bucket;
    public float maxBucket;
    public BucketState bucketState;

    public float refractoryTime;
    public float maxRefractoryTime;
    public float refractorySpeed;

    public float orgasmTime;
    public float maxOrgasmTime;

    public event EventHandler<OnBucketStateChangedEventArgs> OnBucketStateChanged;
    public class OnBucketStateChangedEventArgs : EventArgs
    {
        public BucketState state;
    }

    private void Awake()
    {
        LoadGameValues();
    }
    private void Start()
    {
        if (bucket >= maxBucket)
            UpdateBucket(BucketState.Full);
        else UpdateBucket(BucketState.Empty);
    }

    private void LoadGameValues() //later this will have to implement savestates
    {
        maxArousal = 70;
        decayArousal = 1;

        ratioBuildup = 0.2f;
        maxBuildup = 100;

        maxBucket = 70;
        
        maxRefractoryTime = 5;
        maxOrgasmTime = 3;

        arousal = 0;
        fluid = 0;
        buildup = 0;
        bucket = 0;
        refractoryTime = 0;
        orgasmTime = 0;
    }

    private void FixedUpdate() //formulas to later tune For His Pleasure
    {
        if (orgasmTime > 0 || refractoryTime > 0) return;

        buildup = Mathf.Min(maxBuildup, buildup + arousal * ratioBuildup * Mathf.Pow(arousal / maxArousal, 3) * Time.fixedDeltaTime);

        arousal = Mathf.Max(0, arousal - decayArousal * Time.fixedDeltaTime); 
    }

    internal void EmptyBucket()
    {
        fluid += Mathf.RoundToInt(bucket);
        bucket = 0;
        UpdateBucket(BucketState.None);
    }
    internal void UpdateBucket(BucketState state)
    {
        if(bucketState != state)
        {
            bucketState = state;
            OnBucketStateChanged?.Invoke(this, new OnBucketStateChangedEventArgs { state = state });
        }
    }

    internal IEnumerator Orgasm()
    {
        var halfOrgasmTime = maxOrgasmTime * 0.5f;
        
        refractorySpeed = maxArousal / maxRefractoryTime;
        ;
        float emptyingSpeed = maxBuildup / halfOrgasmTime;
        float emptyingPerTick = emptyingSpeed * tick;
        for (orgasmTime = halfOrgasmTime; orgasmTime > 0; orgasmTime -= tick)
        {
            if (buildup > 0)
            {
                buildup -= emptyingPerTick;
                if(bucket < maxBucket)
                {
                    bucket = Mathf.Min(maxBucket, bucket + emptyingPerTick);
                    if(bucket == maxBucket)
                    {
                        UpdateBucket(BucketState.Full);
                    }
                }
            }
            yield return new WaitForSeconds(tick);
        }
        buildup = 0;
        for (orgasmTime = halfOrgasmTime; orgasmTime > 0; orgasmTime -= tick)
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

public enum BucketState { None, Empty, Full }