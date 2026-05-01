using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ItemsDetails
{
    public int itemCode;
    public ItemType itemType;
    public string itemDescription;
    public Sprite itemsSprite;
    public string itemLongDescription;
    public short itemUsedGridRadius;
    public float itemUsedRadius;
    public bool isStartingItem;
    public bool canBePickUp;
    public bool canBeDropped;
    public bool canBeEaten;
    public bool canBeCarried;
}
