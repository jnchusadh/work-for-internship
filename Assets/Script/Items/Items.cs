using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    [SerializeField]
    private int items_Code;
    private SpriteRenderer spriteRenderer;
    public int Items_code
    {
        get
        {
            return items_Code;
        }
        set
        {
            items_Code = value;
        }
    }
    private void Awake()
    {
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
    }
    private void Start()
    {
        if (Items_code != 0)
        {
            Init(Items_code);
        }
    }

    public void Init(int itemscode)
    {
        if (itemscode != 0)
        {
            items_Code = itemscode;
            ItemsDetails itemsDetails = InventoryManager.Instance.GetItemsDetails(items_Code);
            spriteRenderer.sprite = itemsDetails.itemsSprite;
        }
    }
}
