using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GridProperty 
{
    public GridCoordinate gridCoordinate;
    public GridBoolProperty gridBoolProperty;
    public bool gridBoolValue = false;

    public GridProperty(GridCoordinate gridCoordinate, GridBoolProperty girdBoolProperty,bool gridBoolValue)
    {
        this.gridCoordinate = gridCoordinate;
        this.gridBoolProperty = girdBoolProperty;
        this.gridBoolValue = gridBoolValue;
    }
}
