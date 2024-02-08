using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickManager : MonoBehaviour
{
    private Camera cam;

    [SerializeField] private ClickData clickData;

    void Start()
    {
        cam = Camera.main;
    }

    private RaycastHit2D hit;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hit = Physics2D.GetRayIntersection(cam.ScreenPointToRay(Input.mousePosition));
            
            if (hit != default) hit.collider.GetComponent<ErogenousArea>()?.Stimulate(clickData);
        }
    }
}
