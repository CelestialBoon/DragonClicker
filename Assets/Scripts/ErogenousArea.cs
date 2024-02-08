using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ErogenousArea : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    public ErogenousType type;
    public UnityEvent OnStimulated = new UnityEvent();
    public float arousalSpeed, fluidSpeed = 1;
    
    public virtual void Stimulate(ClickData clickData)
    {
        OnStimulated.Invoke();
        /*if (gameState.orgasmTime > 0) return;
        float arousalChange = 1;
        if (gameState.buildup >= gameState.maxBuildup) arousalChange *= 5;
        arousalSpeed = arousalChange * 5;
        if (gameState.refractoryTime > 0)
        {
            arousalChange *= 0.2f;
        }

        gameState.arousal += arousalChange;

        if(gameState.arousal >= gameState.maxArousal) //orgasm time
        {
            StartCoroutine(gameState.Orgasm());
            //fluidSpeed = (gameState.fluid - fluidDisplayed) * 2 / gameState.maxOrgasmTime;
        }*/
    }
}
public enum ErogenousType {COCK, BALLS, ANUS, PAW, BELLY, HEAD, MOUTH, OTHER}
