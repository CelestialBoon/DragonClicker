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

    void OnEnable() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button button = root.Q<Button>("Button");
        Label label = root.Q<Label>("Label");
        ProgressBar progressBar = root.Q<ProgressBar>("ProgressBar");
        
        label.text = GetLabelText(0);
        progressBar.lowValue = 0;
        progressBar.highValue = maxCounter;

        button.clicked += () => {
            counter++;
            progressBar.value = counter;
            if (counter >= maxCounter) {
                counter = 0;
                points += 100;
                label.text = GetLabelText(points);
            }
        };
    }

    public static string GetLabelText(int points)
    {
        return $"Cum: {points}ml";
    }
}
