using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="so_GridProperties",menuName ="Scriptable Objects/Grid Properties")]
public class SO_GridProperty : ScriptableObject
{
    public SceneName sceneName;
    public int gridWidth;
    public int gridHeight;
    public int orignX;
    public int orignY;
    [SerializeField]
    public List<GridProperty> gridPropertyList;
}
