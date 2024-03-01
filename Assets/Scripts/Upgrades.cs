using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
    public List<Upgrade> upgradeList;
    public Dictionary<string, int> nameDict;

    private void Awake()
    {
        upgradeList = new List<Upgrade>();
        nameDict = new Dictionary<string, int>();
        AddUpgrade(new Upgrade {
            maxTier = 4,
            codeName = "bucket",
            displayName = "Better Bucket",
            iconName = "bucketFull",
            effect = (u, gd) => {
                gd.bucketCapture += 0.3f;
                Debug.Log($"Tier {u.tier} bucket bought. Bucket capture now {gd.bucketCapture:n2}"); 
            },
            otherRequirements = NoRequirements,
            baseMoneyCost = 50,
            moneyMultiplier = 3f,
        }); 
        AddUpgrade(new Upgrade {
            maxTier = 6,
            codeName = "kobold",
            displayName = "Recruit a Kobold",
            iconName = "kobold",
            effect = (u, gd) => {
                gd.koboldsMax++;
                gd.gs.OnKoboldReassigned.Invoke(null);
            },
            otherRequirements = NoRequirements,
            baseMoneyCost = 100,
            moneyMultiplier = 3f,
        });
        AddUpgrade(new Upgrade {
            maxTier = 2,
            codeName = "koboldSpeed",
            displayName = "Train Kobolds for Speed",
            iconName = "kobold",
            effect = (u, gd) => {
                gd.koboldDelay--;
            },
            otherRequirements = NoRequirements,
            baseMoneyCost = 100,
            moneyMultiplier = 10f,
        });
        AddUpgrade(new Upgrade {
            maxTier = 2,
            codeName = "koboldStrength",
            displayName = "Train Kobolds for Strength",
            iconName = "kobold",
            effect = (u, gd) => {
                gd.koboldStrength += 0.3f;
            },
            otherRequirements = NoRequirements,
            baseMoneyCost = 100,
            moneyMultiplier = 10f,
        });
        AddUpgrade(new Upgrade {
            maxTier = 5,
            codeName = "strength",
            displayName = "Train your own Strength",
            iconName = "upgrade",
            effect = (u, gd) => {
                gd.clickStrength += 0.3f;
            },
            otherRequirements = NoRequirements,
            baseMoneyCost = 100,
            moneyMultiplier = 5f,
        });
        AddUpgrade(new Upgrade
        {
            maxTier = 5,
            codeName = "dragonUpgrade",
            displayName = "Dragon Lust Potion",
            iconName = "upgrade",
            effect = (u, gd) => {
                gd.maxArousal *= 3;
                gd.cumQuality++;
            },
            otherRequirements = NoRequirements,
            baseMoneyCost = 500,
            moneyMultiplier = 5f,
        });
        AddUpgrade(new Upgrade {
            maxTier = 1,
            isInstrument = true,
            codeName = "vibrator",
            displayName = "Vibrator",
            iconName = "vibrator",
            effect = UnlockInstrument,
            otherRequirements = NoRequirements,
            baseMoneyCost = 100,
        }); 
        AddUpgrade(new Upgrade {
            maxTier = 1,
            isInstrument = true,
            codeName = "dildo",
            displayName = "Dildo",
            iconName = "dildo",
            effect = UnlockInstrument,
            otherRequirements = NoRequirements,
            baseMoneyCost = 100,
        });
    }

    private bool NoRequirements(GameData _) { return true; }

    private void UnlockInstrument(Upgrade u, GameData gd)
    {
        UI.Instance.AddInstrument(u);
    }

    private void AddUpgrade(Upgrade u)
    {
        u.UpdateCosts();
        u.index = upgradeList.Count;
        upgradeList.Add(u);
        nameDict.Add(u.codeName, u.index);
    }
}

[Serializable]
public class Upgrade
{
    public int index;
    public int tier = 0;
    public int maxTier = 1;
    public string codeName;
    public string displayName;
    public string iconName;
    public bool isInstrument = false;
    public int baseMoneyCost = 100;
    public float moneyMultiplier = 2;
    public int baseCumCost = 0;
    public float cumMultiplier = 2;
    public int baseCumQuality = 0;
    public float cumQualityIncrease = 0;
    public string tooltipText;
    public string boughtText;
    public Action<Upgrade, GameData> effect;
    public Func<GameData, bool> otherRequirements;
    public bool isInShop = false;
    public bool isBoughtOut = false;
    public int nextGoldCost = 0;
    public int nextCumCost = 0;
    public int nextCumQuality = 0;
    public Upgrade() { }

    public void UpdateCosts()
    {
        nextGoldCost = Mathf.FloorToInt(baseMoneyCost * Mathf.Pow(moneyMultiplier, tier));
        if (!IsCumCost()) return;
        nextCumCost = Mathf.FloorToInt(baseCumCost * Mathf.Pow(moneyMultiplier, tier));
        nextCumQuality = Mathf.FloorToInt(baseCumQuality + cumQualityIncrease * tier);
    }
    public bool IsCumCost() { return baseCumCost > 0; }

    public bool IsNearRequirements(GameData gd)
    {
        return (gd.gold >= nextGoldCost * 0.6f) && (!IsCumCost() || gd.cumQuality >= nextCumQuality);
    }

    public bool IsEnoughGoldRequirements(GameData gd)
    {
        return gd.gold >= nextGoldCost;
    }
    public bool IsEnoughCumRequirements(GameData gd)
    {
        if (!IsCumCost()) return true;
        
        return gd.cumQuality >= nextCumQuality && gd.cum >= nextCumCost;
    }
}