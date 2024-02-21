using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Instrument", fileName = "New Instrument")]
public class InstrumentSO : ScriptableObject
{
    [EnumNamedArray(typeof(ErogenousType))]
    public float[] effectiveness;
    public bool bought;
    public ErogenousType? assigned;
}
