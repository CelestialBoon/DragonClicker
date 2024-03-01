using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//this class holds the mechanical game data that gets saved and loaded
//as well as the mechanics to update it
[Serializable]
public class GameData
{
    public ErogenousData[] erogenousDatas;

    public Dictionary<string, int> boughtUpgrades;

    public float clickStrength = 1;
    public float koboldStrength = 0.4f;
    public float koboldDelay = 3f;
    public int koboldsBusy = 0;
    public int koboldsMax = 1;
    public bool canKoboldsUseInstruments = false;

    public bool[] koboldsToEZ;
    public string[] instrumentToEz;
    public Dictionary<string, int> whereInstrument;

    public float arousal = 0;
    public float maxArousal = 100;
    public float decayArousal = 0.03f;

    public int cum = 0;
    public int cumQuality = 0;

    public float buildup = 0;
    public float arousal2Buildup = 10;
    public float ratioBuildup = 9f;
    public float passiveBuildup = 2f;
    public float refractoryBuildup = 10f;
    public float maxBuildup = 100;

    public float bucket = 0;
    public float bucketCapture = 0.3f;
    public BucketState bucketState;

    public float refractoryTime = 0;
    public float maxRefractoryTime = 4;
    public float refractorySpeed;

    public float orgasmTime = 0;
    public float maxOrgasmTime = 3;

    public int gold;

    public GameState gs;

    public GameData Initialize(GameState gameState)
    {
        gs = gameState;
        boughtUpgrades = new Dictionary<string, int>();

        int erogenousCount = Enum.GetNames(typeof(ErogenousType)).Length;
        erogenousDatas = new ErogenousData[erogenousCount];
        erogenousDatas[(int)ErogenousType.COCK] = new ErogenousData(1.5f, 3, 0.2f);
        erogenousDatas[(int)ErogenousType.BALLS] = new ErogenousData(0.5f, 5, 0.8f);
        erogenousDatas[(int)ErogenousType.ANUS] = new ErogenousData(1f, 5, 0.6f);
        erogenousDatas[(int)ErogenousType.PAW] = new ErogenousData(-0.5f, 5, 1f);
        erogenousDatas[(int)ErogenousType.WING] = new ErogenousData(-1f, 10, 3f);
        erogenousDatas[(int)ErogenousType.HEAD] = new ErogenousData(0f, 2, 0.5f);
        erogenousDatas[(int)ErogenousType.MOUTH] = new ErogenousData(-1, 5, 0.7f);

        koboldsToEZ = new bool[erogenousCount];
        instrumentToEz = new string[erogenousCount];
        whereInstrument = new Dictionary<string, int>(); //it gets populated only on unlock

        bucketState = BucketState.Empty;
        return this;
    }

   

    public void Start_()
    {
        if (bucket >= 0) EmptyBucket();
        UpdateBucket(BucketState.Empty);
    }

    public void FixedUpdate_() //formulas to later tune For His Pleasure
    {
        if (orgasmTime > 0 || refractoryTime > 0) return;

        for (int i = 0; i < erogenousDatas.Length; i++)
        {
            ErogenousData ed = erogenousDatas[i];
            if (ed.HasKobold && ed.TimeSinceLast > ed.OptimalTime * koboldDelay)
            {
                gs.erogenousAreas[i].StimulateWithKobold();
            }
        }

        IncreaseBuildup(passiveBuildup * (1 + ratioBuildup * Mathf.Pow(arousal / maxArousal, 2)) / (1+ratioBuildup) * Time.fixedDeltaTime);

        arousal = Mathf.Max(0, arousal - decayArousal * maxArousal * Time.fixedDeltaTime);

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

    internal void BuyUpgrade(Upgrade u)
    {
        int prevGold = gold;
        u.isInShop = false;
        gold -= u.nextGoldCost;
        gs.OnGoldChanged.Invoke(prevGold);
        if(u.IsCumCost())
        {
            int prevCum = cum;
            cum -= u.nextCumCost;
            gs.OnCumChanged.Invoke(prevCum);
        }
        u.effect(u, this);
        boughtUpgrades.Put(u.codeName, u.tier);
        u.tier++;
    }

    public void Arouse(float amount)
    {
        arousal += amount;
        IncreaseBuildup(amount * arousal2Buildup / maxArousal);

        if (arousal > maxArousal) gs.OnOrgasm.Invoke();
    }

    internal void EmptyBucket()
    {
        int prevCum = cum;
        cum += Mathf.RoundToInt(bucket);
        gs.OnCumChanged.Invoke(prevCum);
        bucket = 0;
        //UpdateBucket(BucketState.None);
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
                bucket += emptyingPerTick * bucketCapture;
            }
            yield return new WaitForFixedUpdate();
        }
        buildup = 0;
        EmptyBucket();
        //UpdateBucket(BucketState.Full);
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
        int prevGold = gold;
        int prevCum = cum;
        gold += Mathf.RoundToInt(cum * Mathf.Pow(2, cumQuality));
        cum = 0;
        gs.OnCumChanged.Invoke(prevCum);
        gs.OnGoldChanged.Invoke(prevGold);
    }

    internal void IncreaseBuildup(float amount)
    {
        buildup = Mathf.Min(maxBuildup, buildup + amount);
    }

    internal ErogenousData GetErogenousData(ErogenousType type)
    {
        return erogenousDatas[(int)type];
    }

    internal void SwapInstrument(string instrName, int edIndex)
    {
        DetachInstrument(edIndex);
        AttachInstrument(instrName, edIndex);
    }

    internal void AttachInstrument(string instrName, int edIndex)
    {
        InstrumentSO instrument = gs.instrumentsDict[instrName];
        ErogenousData ed = erogenousDatas[edIndex];
        ed.KoboldInstrument = instrument;
        instrumentToEz[edIndex] = instrument.codeName;
        whereInstrument[instrName] = edIndex;
    }

    internal void DetachInstrument(int index)
    {
        ErogenousData ed = erogenousDatas[index];
        string instrName = ed.KoboldInstrument?.codeName;
        if (!string.IsNullOrEmpty(instrName))
        {
            ed.KoboldInstrument = null;
            instrumentToEz[index] = null;
            whereInstrument[instrName] = -1;
        }
    }
}
