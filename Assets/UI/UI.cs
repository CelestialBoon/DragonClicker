using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    private int counter = 0;
    private int maxCounter = 20;
    private int points = 0;
    private bool hasReachedMax = false;
    private VisualElement root;
    private Button cockJerkButton;
    private ProgressBar arousalProgressBar;

    private void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        cockJerkButton = root.Q<Button>("Button");
        arousalProgressBar = root.Q<ProgressBar>("ProgressBar");
    }


    void OnEnable()
    {
        Label label = root.Q<Label>("Label");

        label.text = GetLabelText(0);
        arousalProgressBar.lowValue = 0;
        arousalProgressBar.highValue = maxCounter;

        cockJerkButton.clicked += () =>
        {
            counter++;
            arousalProgressBar.value = counter;
            if (counter >= maxCounter)
            {
                points += 100;
                label.text = GetLabelText(points);
            }
        };
    }

    private void Update()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        ProgressBar progressBar = root.Q<ProgressBar>("ProgressBar");
        if (counter >= maxCounter && hasReachedMax == false)
        {
            hasReachedMax = true;
        }
        if (hasReachedMax == true)
        {
            progressBar.value -= 5 * Time.deltaTime;
        }
        if (counter == 0 && hasReachedMax == true)
        {
            hasReachedMax = false;
        }

    }


    public static string GetLabelText(int points)
    {
        return $"Cum: {points}ml";
    }
}
