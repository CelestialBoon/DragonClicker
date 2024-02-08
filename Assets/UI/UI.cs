using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] GameState gs;

    private float arousalDisplayed;
    private int fluidDisplayed;
    private int goldDisplayed;
    private float arousalSpeed = 1;
    private float fluidSpeed = 1;

    private VisualElement root;
    private Button penisButton;
    private Button bucketButton;
    private Button sellButton;
    private ProgressBar arousalProgressBar;
    private ProgressBar buildupProgressBar;
    private Label fluidLabel;
    private Label goldLabel;
    [SerializeField] private Texture2D bucketEmpty;
    [SerializeField] private Texture2D bucketFull;


    //later when this code grows in length and complexity, the code that handles the actual game state values (instead of the displayed ones) will have to be put elsewhere

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        penisButton = root.Q<Button>("PenisButton");
        bucketButton = root.Q<Button>("Bucket");
        arousalProgressBar = root.Q<ProgressBar>("ArousalProgress");
        buildupProgressBar = root.Q<ProgressBar>("BuildupProgress");
        fluidLabel = root.Q<Label>("FluidLabel");
        goldLabel = root.Q<Label>("GoldLabel");
        sellButton = root.Q<Button>("SellCumButton");

        arousalDisplayed = gs.arousal;

        fluidLabel.text = GetFluidLabelText(gs.fluid);
        goldLabel.text = GetGoldLabelText(gs.gold);
        arousalProgressBar.lowValue = 0;
        arousalProgressBar.highValue = gs.maxArousal;
        buildupProgressBar.lowValue = 0;
        buildupProgressBar.highValue = gs.maxBuildup;

        gs.OnAroused.AddListener(UpdateArousalSpeed);
        gs.OnOrgasm.AddListener(UpdateFluidSpeed);

        gs.OnBucketStateChanged += (sender, ea) => { ChangeBucketAppearance(ea.state); };

        bucketButton.clicked += () =>
        {
            if (gs.bucketState != BucketState.None)
            {
                gs.EmptyBucket();
            }
            else
            {
                gs.UpdateBucket(BucketState.Empty);
            }
        };

        sellButton.clicked += () =>
        {
            gs.sellCum();
        };
    }

    private void Update()
    {
        if (gs.fluid > fluidDisplayed)
        {
            fluidDisplayed += Utils.RoundP((gs.fluid - fluidDisplayed) * fluidSpeed * Time.deltaTime);
            fluidLabel.text = GetFluidLabelText(fluidDisplayed);
        }
        else if (gs.fluid < fluidDisplayed)
        {
            fluidDisplayed -= Utils.RoundP((gs.fluid - fluidDisplayed) * fluidSpeed * Time.deltaTime);
            fluidLabel.text = GetFluidLabelText(fluidDisplayed);
        }
        if (gs.gold > goldDisplayed)
        {
            goldDisplayed += Utils.RoundP((gs.gold - goldDisplayed) * fluidSpeed * Time.deltaTime);
            goldLabel.text = GetGoldLabelText(goldDisplayed);
        }
        else if (gs.gold < goldDisplayed)
        {
            goldDisplayed -= Utils.RoundP((gs.gold - goldDisplayed) * fluidSpeed * Time.deltaTime);
            goldLabel.text = GetGoldLabelText(goldDisplayed);
        }
        if (!Mathf.Approximately(gs.arousal, arousalDisplayed))
        {
            arousalDisplayed = Mathf.MoveTowards(arousalDisplayed, gs.arousal, arousalSpeed);
            arousalProgressBar.value = arousalDisplayed;
            arousalProgressBar.title = $"Arousal: {gs.arousal.ToString("0")}/{gs.maxArousal}";
        }
        buildupProgressBar.value = gs.buildup;
        buildupProgressBar.title = $"Buildup: {gs.buildup.ToString("0")}/{gs.maxBuildup}";
    }

    public void UpdateArousalSpeed(float arousalChange)
    {
        arousalSpeed = arousalChange * 5;
    }
    public void UpdateFluidSpeed()
    {
        fluidSpeed = (gs.fluid - fluidDisplayed) * 2 / gs.maxOrgasmTime;
    }
    public static string GetFluidLabelText(int points)
    {
        return $"Cum: {points}ml";
    }
    public static string GetGoldLabelText(int points)
    {
        return $"Gold: {points}";
    }

    private void ChangeBucketAppearance(BucketState state)
    {
        bucketButton.style.opacity = state switch
        {
            BucketState.None => 0,
            _ => 1
        };
        bucketButton.style.backgroundImage = state switch
        {
            BucketState.Full => bucketFull,
            _ => bucketEmpty,
        };
    }
}

