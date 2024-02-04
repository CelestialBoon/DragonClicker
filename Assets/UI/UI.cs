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
    private Label cumLabel;

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        cockJerkButton = root.Q<Button>("Button");
        arousalProgressBar = root.Q<ProgressBar>("ProgressBar");
        cumLabel = root.Q<Label>("Label");


        cumLabel.text = GetLabelText(0);
        arousalProgressBar.lowValue = 0;
        arousalProgressBar.highValue = maxCounter;

        cockJerkButton.clicked += () =>
        {
            if (hasReachedMax == false)
            {
                counter++;
                arousalProgressBar.value = counter;
                if (counter >= maxCounter)
                {
                    points += 100;
                    cumLabel.text = GetLabelText(points);
                }
            }
        };
    }

    private void Update()
    {

        if (counter >= maxCounter && hasReachedMax == false)
        {
            hasReachedMax = true;
        }
        if (hasReachedMax == true)
        {
            arousalProgressBar.value -= 5 * Time.deltaTime;
        }
        if (arousalProgressBar.value <= 0 && hasReachedMax == true)
        {
            hasReachedMax = false;
            arousalProgressBar.value = 0;
            counter = 0;
        }

    }


    public static string GetLabelText(int points)
    {
        return $"Cum: {points}ml";
    }
}
