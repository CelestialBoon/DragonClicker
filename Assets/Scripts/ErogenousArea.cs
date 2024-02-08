using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ErogenousArea : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    public ErogenousType type;
    public UnityEvent OnStimulated = new UnityEvent();
    
    public virtual void Stimulate(ClickData clickData)
    {
        OnStimulated.Invoke();
        gameState.Arouse(clickData.parameters.baseStrength * clickData.multiplier, type);
    }
}
public enum ErogenousType {COCK, BALLS, ANUS, PAW, BELLY, HEAD, MOUTH, OTHER}
