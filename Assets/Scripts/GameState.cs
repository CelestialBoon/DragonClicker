using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

//responsibilities of this class:
//holder of game data and saver/loader and event launcher for it
//handles menus state machine
//pauses and unpauses
public class GameState : MonoBehaviour 
{
    [SerializeField] private ClickData clickData; //TODO very temporary, to be reworked once kobolds and tools are introduced

    [NonSerialized] public GameData gameData;

    public UnityEvent OnOrgasm = new UnityEvent();
    public UnityEvent OnRefractory = new UnityEvent();
    public UnityEvent<float> OnAroused = new UnityEvent<float>();
    public UnityEvent<BucketState> OnBucketStateChanged = new UnityEvent<BucketState>();
    public UnityEvent OnGameDataUpdated = new UnityEvent();

    private string savePath;

    private void Start()
    {
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
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (hit != default) hit.collider.GetComponent<ErogenousArea>()?.Stimulate(clickData);
        }
    }

    private void FixedUpdate()
    {
        gameData.FixedUpdate_();
    }

    internal ClickData GetManualClickData(ErogenousType type)
    {
        return clickData; //TODO this is a stub for now
    }


    public void PauseGame(bool isPause)
    {
        if (isPause) Time.fixedDeltaTime = 0;
        else Time.fixedDeltaTime = 0.02f;
    }

    public bool IsPaused() { return Time.fixedDeltaTime != 0; }

    //here goes loading and saving
    public void SaveGame()
    {
        //gather all data and write to file
        gameData.gs = null;
        File.WriteAllText(savePath, JsonUtility.ToJson(gameData));
        gameData.gs = this;
    }
    public void LoadGame()
    {
        GameData newGameData = null;
        //find path
        if (File.Exists(savePath))
        {
            //take the json and create a new gamedata and replace the old one
            newGameData = JsonUtility.FromJson<GameData>(File.ReadAllText(savePath));
        }

        if(newGameData == null)
        {
            //initialize gamedata if path not found
            newGameData = new GameData().Initialize(this);
        }
        gameData.CopyProperties(newGameData);
        OnGameDataUpdated.Invoke();
    }
}

public enum UIState { MainMenu, Options, PlayScreen }
public enum BucketState { None, Empty, Full }