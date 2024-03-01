using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using TMPro;

//responsibilities of this class:
//holder of game data and saver/loader and event launcher for it
//handles menus state machine
//pauses and unpauses
public class GameState : MonoBehaviour
{
    public static GameState Instance;

    [SerializeField] public Upgrades upgrades;

    public GameData gameData;

    [NonSerialized] public ErogenousArea[] erogenousAreas;

    [SerializeField] List<IconSO> iconsList;
    public Dictionary<string, Sprite> iconsDict;

    public List<InstrumentSO> instrumentsList;
    public Dictionary<string, InstrumentSO> instrumentsDict; 
    public string instrumentInHand;
    public TMPro.TextMeshPro counterTemplate;

    public UnityEvent OnOrgasm = new();
    public UnityEvent OnRefractory = new();
    public UnityEvent<ErogenousArea, float> OnAroused = new();
    public UnityEvent<BucketState> OnBucketStateChanged = new();
    public UnityEvent OnGameDataLoaded = new();
    public UnityEvent<ErogenousArea> OnKoboldReassigned = new();
    public UnityEvent<int> OnGoldChanged = new();
    public UnityEvent<int> OnCumChanged = new();
    public UnityEvent OnReset = new();

    private string savePath;

    private void Awake()
    {
        Instance = this;
        erogenousAreas = new ErogenousArea[Enum.GetNames(typeof(ErogenousType)).Length];
    }

    private void Start()
    {
        iconsDict = new();
        foreach (IconSO iconSO in iconsList)
        {
            iconsDict.Add(iconSO.codeName, iconSO.sprite);
        }
        instrumentsDict = new();
        foreach(InstrumentSO instrument in instrumentsList)
        {
            instrumentsDict.Add(instrument.codeName, instrument);
        }
        gameData = new GameData().Initialize(this);
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
            //TODO temporary measure until load button is implemented
            LoadGame();
        } catch (Exception e)
        {
            Debug.LogError($"Couldn't manage to load game from {savePath}");
        }
        OnOrgasm.AddListener(() => StartCoroutine(gameData.Orgasm()));
        OnRefractory.AddListener(() => StartCoroutine(gameData.RefractoryPeriod()));
        OnAroused.AddListener((ea, str) => {
            gameData.Arouse(str);
            CreatePleasureCounter(ea, str);
        });
    }

    private void CreatePleasureCounter(ErogenousArea ea, float strength)
    {
        GameObject go = Instantiate(counterTemplate.gameObject);
        go.transform.SetParent(ea.textAnchor.transform);
        go.transform.localPosition = Vector3.zero;
        TextMeshPro text = go.GetComponent<TextMeshPro>();
        text.text = $"+{strength:n1}";
        StartCoroutine(FloatText(go, ea));
    }

    IEnumerator FloatText(GameObject textGO, ErogenousArea ea)
    {
        float height = 0.7f;
        float progress = 0;
        float speed = height * 0.7f / gameData.erogenousDatas[(int)ea.type].OptimalTime;
        float timeCoeff = ea.lastTimeCoeff;
        byte red = 0, blue = 0, green = 0;
        green = (byte)Math.Min(255, timeCoeff * 510);
        if (timeCoeff <= 0.5f) red = (byte)(255 - timeCoeff * 510);
        else red = (byte)(timeCoeff * 510 - 255);

        byte opacity = 255;
        TextMeshPro text = textGO.GetComponent<TextMeshPro>();
        yield return new WaitForEndOfFrame();
        while(progress < height)
        {
            opacity = (byte)(255 - Mathf.FloorToInt(progress / height * 255));
            text.faceColor = new Color32(red, green, blue, opacity);
            var movement = speed * Time.deltaTime;
            progress += movement;
            textGO.transform.position += new Vector3(0, movement, 0);
            yield return new WaitForEndOfFrame();
        }
        Destroy(textGO);
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

        ErogenousArea ea = hit != default ? hit.collider.GetComponent<ErogenousArea>() : null;
        if(string.IsNullOrEmpty(instrumentInHand)) {
            if(ea != null)
            {
                var instrument = instrumentsDict["handHovering"];
                Cursor.SetCursor(instrument.cursor, new Vector2(16, 6), CursorMode.Auto);
            } else
            {
                var instrument = instrumentsDict["handIdle"];
                Cursor.SetCursor(instrument.cursor, new Vector2(16, 6), CursorMode.Auto);
            }
        }
        if(ea != null) {
            bool leftButton = Input.GetMouseButtonDown(0);
            bool rightButton = Input.GetMouseButtonDown(1);
            if (leftButton || rightButton)
            {
                if (leftButton) ea.Stimulate(gameData.clickStrength, GetInstrumentInHand());
                else if(rightButton)
                {
                    int type = (int)ea.type;
                    if(! string.IsNullOrEmpty(instrumentInHand) && gameData.erogenousDatas[type].HasKobold && gameData.canKoboldsUseInstruments) {
                        //add or swap instruments
                        gameData.SwapInstrument(instrumentInHand, type);
                    } else { //add/remove kobolds
                        if (gameData.erogenousDatas[type].HasKobold) {
                            gameData.erogenousDatas[type].HasKobold = false;
                            gameData.koboldsBusy--;
                            gameData.DetachInstrument(type);

                            if (ea.koboldAnimation != null) {
                                ea.koboldAnimation.SetActive(false); }
                            OnKoboldReassigned.Invoke(ea);
                        } else if (gameData.koboldsBusy < gameData.koboldsMax) {
                            gameData.erogenousDatas[type].HasKobold = true;
                            gameData.koboldsBusy++;
                            if (ea.koboldAnimation != null) {
                                ea.koboldAnimation.SetActive(true); }
                            OnKoboldReassigned.Invoke(ea);
                        }
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        gameData.FixedUpdate_();
    }

    private InstrumentSO GetInstrumentInHand()
    {
        if (string.IsNullOrEmpty(instrumentInHand)) return null;
        else return instrumentsDict[instrumentInHand];
    }

    internal void ChangeInstrument(string instrName)
    {
        if (instrumentInHand == instrName || string.IsNullOrEmpty(instrName)) {
            instrumentInHand = null;
            var instrument = instrumentsDict["handIdle"];
            Cursor.SetCursor(instrument.cursor, new Vector2(16,6), CursorMode.Auto);
        } else {
            instrumentInHand = instrName;
            var instrument = instrumentsDict[instrName];
            Cursor.SetCursor(instrument.cursor, new Vector2(47,14), CursorMode.Auto);
        }
    }

    public void PauseGame(bool isPause)
    {
        if (isPause) Time.fixedDeltaTime = 0;
        else Time.fixedDeltaTime = 0.02f;
    }

    public bool IsPaused() { return Time.fixedDeltaTime != 0; }

    public void SaveGame()
    {
        /*
        //gather all data and write to file
        gameData.gs = null;
        gameData.koboldsToEZ = gameData.erogenousDatas.Select(ed => ed.HasKobold).ToArray();
        gameData.instrumentToEz = gameData.erogenousDatas.Select(ed => ed.KoboldInstrument?.codeName).ToArray();
        //File.WriteAllText(savePath, System.Text.Json. (gameData));
        gameData.gs = this;
        */
    }
    public void LoadGame()
    {
        /*
        //find path
        if (!File.Exists(savePath)) return;
        //take the json and create a new gamedata and replace the old one
        JsonUtility.FromJsonOverwrite(File.ReadAllText(savePath), gameData);
        gameData.gs = this;
        for(int i = 0; i<erogenousAreas.Length; i++) {
            ErogenousData ed = gameData.erogenousDatas[i];
            ed.HasKobold = gameData.koboldsToEZ[i];
            string instr = gameData.instrumentToEz[i];
            if(!string.IsNullOrEmpty(instr)) {
                ed.KoboldInstrument = instruments[instr];
            }
        }
        foreach(var kv in gameData.boughtUpgrades)
        {
            upgrades.upgradeList[upgrades.nameDict[kv.Key]].tier = kv.Value;
            Debug.Log("Aaaa");
        }
        OnGameDataLoaded.Invoke();
        */
    }
}

public enum UIState { MainMenu, Options, PlayScreen }
public enum BucketState { None, Empty, Full }