using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Icon", fileName = "New Icon")]
public class IconSO : ScriptableObject
{
    public string codeName;
    public Sprite sprite;
}
