using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] GameState gs;
    private GameData gd;

    private float arousalDisplayed;
    private int fluidDisplayed;
    private int goldDisplayed;
    private float arousalSpeed = 1;
    private float fluidSpeed = 1;

    private VisualElement root;
    private Button bucketButton;
    private Button sellButton;
    private Button saveButton; //NOTE this is temporary for testing purposes
    private Button resetButton; //NOTE this is temporary for testing purposes
    private Button bucketUpgradeButton;
    private ProgressBar arousalProgressBar;
    private ProgressBar buildupProgressBar;
    private Label fluidLabel;
    private Label goldLabel;
    private Label bucketUpgradeLevel;
    private Label bucketUpgradeCost;
    [SerializeField] private Texture2D bucketEmpty;
    [SerializeField] private Texture2D bucketFull;


    //later when this code grows in length and complexity, the code that handles the actual game state values (instead of the displayed ones) will have to be put elsewhere

    void Start()
    {
        gd = gs.gameData;

        root = GetComponent<UIDocument>().rootVisualElement;
        bucketButton = root.Q<Button>("Bucket");
        arousalProgressBar = root.Q<ProgressBar>("ArousalProgress");
        buildupProgressBar = root.Q<ProgressBar>("BuildupProgress");
        fluidLabel = root.Q<Label>("FluidLabel");
        goldLabel = root.Q<Label>("GoldLabel");
        sellButton = root.Q<Button>("SellCumButton");
        saveButton = root.Q<Button>("SaveButton");
        resetButton = root.Q<Button>("ResetButton");

        bucketUpgradeButton = root.Q<Button>("BucketUpgradeButton");
        bucketUpgradeLevel = root.Q<Label>("BucketUpgradeLevel");
        bucketUpgradeCost = root.Q<Label>("BucketUpgradePrice");

        arousalDisplayed = gd.arousal;

        fluidLabel.text = GetFluidLabelText(gd.fluid);
        goldLabel.text = GetGoldLabelText(gd.gold);
        arousalProgressBar.lowValue = 0;
        arousalProgressBar.highValue = gd.maxArousal;
        buildupProgressBar.lowValue = 0;
        buildupProgressBar.highValue = gd.maxBuildup;

        gs.OnAroused.AddListener(UpdateArousalSpeed);
        gs.OnOrgasm.AddListener(UpdateFluidSpeed);
        gs.OnBucketStateChanged.AddListener(ChangeBucketAppearance);

        bucketButton.clicked += () =>
        {
            if (gd.bucketState != BucketState.None)
            {
                gd.EmptyBucket();
            }
            else
            {
                gd.UpdateBucket(BucketState.Empty);
            }
        };

        bucketUpgradeButton.clicked += () =>
        {
            if (gd.gold >= gd.bucketUpgradePrice)
            {
                gd.gold -= gd.bucketUpgradePrice;
                increaseBucketStorage();
            }
        };

        sellButton.clicked += () =>
        {
            gd.sellCum();
        };

        saveButton.clicked += () => gs.SaveGame();
        resetButton.clicked += () => gs.gameData.Initialize(gs);
    }

    private void Update()
    {
        if (gd.fluid > fluidDisplayed)
        {
            fluidDisplayed += Utils.RoundP((gd.fluid - fluidDisplayed) * fluidSpeed * Time.deltaTime);
            fluidLabel.text = GetFluidLabelText(fluidDisplayed);
        }
        else if (gd.fluid < fluidDisplayed)
        {
            fluidDisplayed -= Utils.RoundP((gd.fluid - fluidDisplayed) * fluidSpeed * Time.deltaTime);
            fluidLabel.text = GetFluidLabelText(fluidDisplayed);
        }
        if (gd.gold > goldDisplayed)
        {
            goldDisplayed += Utils.RoundP((gd.gold - goldDisplayed) * fluidSpeed * Time.deltaTime);
            goldLabel.text = GetGoldLabelText(goldDisplayed);
        }
        else if (gd.gold < goldDisplayed)
        {
            goldDisplayed -= Utils.RoundP((gd.gold - goldDisplayed) * fluidSpeed * Time.deltaTime);
            goldLabel.text = GetGoldLabelText(goldDisplayed);
        }
        if (!Mathf.Approximately(gd.arousal, arousalDisplayed))
        {
            arousalDisplayed = Mathf.MoveTowards(arousalDisplayed, gd.arousal, arousalSpeed);
            arousalProgressBar.value = arousalDisplayed;
            arousalProgressBar.title = $"Arousal: {gd.arousal.ToString("0")}/{gd.maxArousal}";
        }
        buildupProgressBar.value = gd.buildup;
        buildupProgressBar.title = $"Buildup: {gd.buildup.ToString("0")}/{gd.maxBuildup}";

        bucketUpgradeLevel.text = $"LVL {gd.bucketUpgradeLevel}";
        bucketUpgradeCost.text = $"{gd.bucketUpgradePrice}";
    }

    private void increaseBucketStorage()
    {
        gd.upgradeBucket();
        gd.bucketUpgradePrice = Mathf.RoundToInt((float)gd.bucketUpgradePrice * 1.5f) + 100;
    }

    public void UpdateArousalSpeed(float arousalChange)
    {
        arousalSpeed = arousalChange * 5;
    }
    public void UpdateFluidSpeed()
    {
        fluidSpeed = (gd.fluid - fluidDisplayed) * 2 / gd.maxOrgasmTime;
    }
    public static string GetFluidLabelText(int points)
    {
        return $"{points}ml";
    }
    public static string GetGoldLabelText(int points)
    {
        return $"${points}";
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

