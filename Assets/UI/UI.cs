using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] GameState gs;

    private float arousalDisplayed;
    private int fluidDisplayed;
    private float arousalSpeed = 1;
    private float fluidSpeed = 1;

    private VisualElement root;
    private Button penisButton;
    private ProgressBar arousalProgressBar;
    private ProgressBar buildupProgressBar;
    private Label fluidLabel;

    //later when this code grows in length and complexity, the code that handles the actual game state values (instead of the displayed ones) will have to be put elsewhere

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        penisButton = root.Q<Button>("PenisButton");
        arousalProgressBar = root.Q<ProgressBar>("ArousalProgress");
        buildupProgressBar = root.Q<ProgressBar>("BuildupProgress");
        fluidLabel = root.Q<Label>("FluidLabel");

        arousalDisplayed = gs.arousal;

        fluidLabel.text = GetFluidLabelText(gs.fluid);
        arousalProgressBar.lowValue = 0;
        arousalProgressBar.highValue = gs.maxArousal;
        buildupProgressBar.lowValue = 0;
        buildupProgressBar.highValue = gs.maxBuildup;

        penisButton.clicked += () =>
        {
            if (gs.orgasmTime > 0) return;
            float arousalChange = 1;
            if (gs.buildup >= gs.maxBuildup) arousalChange *= 5;
            arousalSpeed = arousalChange * 5;
            if (gs.refractoryTime > 0)
            {
                arousalChange *= 0.2f;
            }

            gs.arousal += arousalChange;

            if(gs.arousal >= gs.maxArousal) //orgasm time
            {
                StartCoroutine(gs.Orgasm());
                fluidSpeed = (gs.fluid - fluidDisplayed) * 2 / gs.maxOrgasmTime;
            }
        };
    }

    private void Update()
    {
        if(gs.fluid != fluidDisplayed)
        {
            fluidDisplayed += Utils.RoundP((gs.fluid - fluidDisplayed) * fluidSpeed * Time.deltaTime);
            fluidLabel.text = GetFluidLabelText(fluidDisplayed);
        }
        if(! Mathf.Approximately(gs.arousal, arousalDisplayed))
        {
            arousalDisplayed = Mathf.MoveTowards(arousalDisplayed, gs.arousal, arousalSpeed);
            arousalProgressBar.value = arousalDisplayed;
            arousalProgressBar.title = $"Arousal: {gs.arousal.ToString("0")}/{gs.maxArousal}";
        }
        buildupProgressBar.value = gs.buildup;
        buildupProgressBar.title = $"Buildup: {gs.buildup.ToString("0")}/{gs.maxBuildup}";
    }
    public static string GetFluidLabelText(int points)
    {
        return $"Cum: {points}ml";
    }
}
