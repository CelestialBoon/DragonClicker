using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class UI : MonoBehaviour
{
    public static UI Instance;

    [SerializeField] GameState gs;
    private GameData gd;

    [SerializeField] VisualTreeAsset shopButton;
    [SerializeField] VisualTreeAsset instrumentButton;
    [SerializeField] VisualTreeAsset boughtUpgradeButton;

    private float arousalDisplayed;
    private int fluidDisplayed;
    private int goldDisplayed;
    private float arousalSpeed = 1;
    private float fluidSpeed = 2f;

    private ScrollView shopScrollView;
    private List<(Button, Upgrade)> visibleUpgrades;
    private VisualElement instrumentsBox;
    private VisualElement boughtUpgradesBox;

    private VisualElement root;
    private Button bucketButton;
    private Button sellButton;
    private Button saveButton;
    private Button loadButton;
    private Button resetButton;
    private ProgressBar arousalProgressBar;
    private ProgressBar buildupProgressBar;
    private Label fluidLabel;
    private Label goldLabel;
    private Label koboldsLabel;
    [SerializeField] private Texture2D bucketEmpty;
    [SerializeField] private Texture2D bucketFull;

    private void Awake()
    {
        Instance = this;
    }

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
        loadButton = root.Q<Button>("LoadButton");
        resetButton = root.Q<Button>("ResetButton");
        koboldsLabel = root.Q<Label>("KoboldsLabel");

        shopScrollView = root.Q<ScrollView>("ShopScrollView");
        instrumentsBox = root.Q<VisualElement>("InstrumentsBox");
        boughtUpgradesBox = root.Q<VisualElement>("BoughtUpgradesBox");

        shopScrollView.Clear();
        visibleUpgrades = new();
        RefreshUpgrades();

        instrumentsBox.Clear();
        boughtUpgradesBox.Clear();

        arousalDisplayed = gd.arousal;

        fluidLabel.text = GetFluidLabelText(gd.cum);
        goldLabel.text = GetGoldLabelText(gd.gold);
        arousalProgressBar.lowValue = 0;
        arousalProgressBar.highValue = gd.maxArousal;
        buildupProgressBar.lowValue = 0;
        buildupProgressBar.highValue = gd.maxBuildup;

        gs.OnAroused.AddListener(UpdateArousalSpeed);
        gs.OnBucketStateChanged.AddListener(ChangeBucketAppearance);

        /*bucketButton.clicked += () =>
        {
            if (gd.bucketState != BucketState.None)
            {
                gd.EmptyBucket();
            }
            else
            {
                gd.UpdateBucket(BucketState.Empty);
            }
        };*/

        UpdateKobolds(null);
        gs.OnKoboldReassigned.AddListener((ea) => {
            UpdateKobolds(ea);
        });
        gs.OnGoldChanged.AddListener(RefreshUpgrades);
        gs.OnCumChanged.AddListener(RefreshUpgrades);

        sellButton.clicked += gd.sellCum;
        saveButton.clicked += gs.SaveGame;
        loadButton.clicked += gs.LoadGame;
        resetButton.clicked += () => gs.OnReset.Invoke();
    }

    private void Update()
    {
        if (gd.cum != fluidDisplayed)
        {
            var diff = gd.cum - fluidDisplayed;
            var speed = Math.Max(Math.Abs(diff), 8) * fluidSpeed;
            fluidDisplayed = Utils.RoundP(Mathf.MoveTowards(fluidDisplayed, gd.cum, speed * Time.deltaTime));
            fluidLabel.text = GetFluidLabelText(fluidDisplayed);
        }
        if (gd.gold != goldDisplayed)
        {
            var diff = gd.gold - goldDisplayed;
            var speed = Math.Max(Math.Abs(diff), 8) * fluidSpeed;
            goldDisplayed = Utils.RoundP(Mathf.MoveTowards(goldDisplayed, gd.gold, speed * Time.deltaTime));
            goldLabel.text = GetGoldLabelText(goldDisplayed);
        }
        if (!Mathf.Approximately(gd.arousal, arousalDisplayed))
        {
            arousalDisplayed = Mathf.MoveTowards(arousalDisplayed, gd.arousal, arousalSpeed);
            arousalProgressBar.value = arousalDisplayed;
            arousalProgressBar.title = $"Arousal: {gd.arousal:0}/{gd.maxArousal}";
        }
        buildupProgressBar.value = gd.buildup;
        buildupProgressBar.title = $"Buildup: {gd.buildup:0}/{gd.maxBuildup}";
    }

    public void RefreshUpgrades(int _ = 0)
    {
        foreach (Upgrade u in gs.upgrades.upgradeList)
        {
            u.UpdateCosts();
            if (u.tier < u.maxTier)
            {
                if (u.IsNearRequirements(gd) && !u.isInShop) 
                    AddUpgrade(u);
            }
            else if (u.isInstrument && !u.isBoughtOut)
            {
                AddInstrument(u);
            }
            else if (!u.isBoughtOut)
            {
                AddBoughtUpgrade(u);
            }
        }
        foreach (var (b, u) in visibleUpgrades)
        {
            UpdateShopItemPrice(b, u);
        }
    }

    private void AddUpgrade(Upgrade u)
    {
        Button b = shopButton.CloneTree().Q<Button>("UpgradeButton");

        var name = b.Q<Label>("UpgradeName");
        var level = b.Q<Label>("UpgradeLevel");
        var icon = b.Q<VisualElement>("UpgradeIcon");

        u.isInShop = true;

        name.text = u.displayName;
        level.text = $"LVL {u.tier}";
        UpdateShopItemPrice(b, u);
        icon.style.backgroundImage = new StyleBackground(gs.iconsDict[u.iconName]);
        //icon.something
        //TODO add icon change here

        b.clicked += () => { 
            if(u.IsEnoughGoldRequirements(gd) && u.IsEnoughCumRequirements(gd) && u.otherRequirements(gd))
            {
                gd.BuyUpgrade(u);
                if (u.tier == u.maxTier) AddBoughtUpgrade(u);
                RemoveUpgrade(b,u);
                RefreshUpgrades();
            }
        };
        int index = visibleUpgrades.Where((a) => a.Item2.nextGoldCost < u.nextGoldCost).Count();
        shopScrollView.Insert(index, b);
        visibleUpgrades.Add((b,u));
    }

    private void RemoveUpgrade(Button b, Upgrade u)
    {
        visibleUpgrades.Remove((b,u));
        b.RemoveFromHierarchy();
    }

    private void UpdateShopItemPrice(Button b, Upgrade u)
    {
        var price = b.Q<Label>("UpgradePrice");
        price.text = $"{u.nextGoldCost}G";
        price.style.color = u.IsEnoughGoldRequirements(gd) ? Color.green : Color.red;
    }

    private void AddBoughtUpgrade(Upgrade u)
    {
        if (u.isInstrument) return;
        u.isBoughtOut = true;
        Button b = boughtUpgradeButton.CloneTree().Q<Button>("UpgradedButton");
        b.style.backgroundImage = new StyleBackground(gs.iconsDict[u.iconName]);
        boughtUpgradesBox.Add(b);
    }

    public void AddInstrument(Upgrade u)
    {
        if (!u.isInstrument) Debug.LogError("upgrade is not instrument");
        u.isBoughtOut = true;
        Button b = instrumentButton.CloneTree().Q<Button>("InstrumentButton");
        b.style.backgroundImage = new StyleBackground(gs.iconsDict[u.iconName]);

        b.clicked += () =>
        {
            gs.ChangeInstrument(u.codeName);
        };

        instrumentsBox.Add(b);
    }

    public void UpdateArousalSpeed(ErogenousArea _, float arousalChange)
    {
        arousalSpeed = arousalChange * 5;
    }
    public void UpdateKobolds(ErogenousArea ez)
    {
        koboldsLabel.text = $"{gd.koboldsBusy}/{gd.koboldsMax}";
    }
    public static string GetFluidLabelText(int points)
    {
        if (points > 1000000) return $"{points * 0.000001f:n2}kl";
        else if (points > 1000) return $"{points * 0.001f:n2}l";
        else return $"{points}ml";
    }
    public static string GetGoldLabelText(int points)
    {
        return $"{points}";
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

