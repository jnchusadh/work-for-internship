using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private Dictionary<int, ItemsDetails> itemDetailDictionary;
    public int[] selectInventoryItem;  
    public List<InventoryItem>[] inventoryLists;
    [HideInInspector] public int[] inventoryListCapcityIntArray;
    [SerializeField] private ScriptableObjectList scriptableObjectList = null;
    protected override void Awake()
    {
        base.Awake();
        CreateItemDictionary();
        CreatInventoryList();
        selectInventoryItem = new int[(int)InventoryLocation.count];
        for(int i = 0; i < selectInventoryItem.Length; i++)
        {
            selectInventoryItem[i] = -1;
        }
    }

    private void CreatInventoryList()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];
        for(int i = 0;i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }
        inventoryListCapcityIntArray = new int[(int)InventoryLocation.count];
        inventoryListCapcityIntArray[(int)InventoryLocation.player] = Settings.inventoryInitialCapcity;
    }

    private void CreateItemDictionary()
    {
        itemDetailDictionary = new Dictionary<int, ItemsDetails>();
        foreach(ItemsDetails itemsDetails in scriptableObjectList.itemsDetails)
        {
            itemDetailDictionary.Add(itemsDetails.itemCode, itemsDetails);
        }
    }
    public void SetSelectInventoryItem(InventoryLocation inventoryLocation,int itemCode)
    {
        selectInventoryItem[(int)inventoryLocation] = itemCode;
    }
    public  void ClearSelectInventoryItem(InventoryLocation inventoryLocation )
    {
        selectInventoryItem[(int)inventoryLocation] = -1;
    }
    public void AddItem(InventoryLocation inventoryLocation, Items items,GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, items);
        Destroy(gameObjectToDelete);
    }
    public void AddItem(InventoryLocation inventoryLocation,Items items)
    {
        int itemCode = items.Items_code;
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        int itemPos = FindItemInList(inventoryLocation, itemCode);
        if (itemPos != -1)
        {
            AddItemInList(inventoryList, itemCode, itemPos);
        }
        else
        {
            AddItemInList(inventoryList, itemCode);
        }
        EventHandler.CallInventoryUpdateEvents(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    private void AddItemInList(List<InventoryItem> inventoryList, int itemCode,int itemPos)
    {
        InventoryItem inventoryItem = new InventoryItem();


        inventoryItem.itemCode = itemCode;
        int quanity = inventoryList[itemPos].itemQuanity + 1;
        inventoryItem.itemQuanity = quanity;
        inventoryList[itemPos] = inventoryItem;
        //DebugPrintInventoryItem(inventoryList);
    }

    private void AddItemInList(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();
        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuanity = 1;
        inventoryList.Add(inventoryItem);
       //DebugPrintInventoryItem(inventoryList);
    }

    public int FindItemInList(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> list = inventoryLists[(int)inventoryLocation];
        for(int i = 0; i < list.Count; i++)
        {
            if(list[i].itemCode == itemCode)
            {
                return i;
            }
        }
        return -1;
    }
    private void RemoveAtPos(List<InventoryItem> inventoryList, int itemCode, int itemPos)
    {
        InventoryItem inventoryItem = new InventoryItem();
        int quanity = inventoryList[itemPos].itemQuanity - 1;
        if (quanity > 0)
        {
            inventoryItem.itemQuanity = quanity;
            inventoryItem.itemCode = itemCode;
            inventoryList[itemPos] = inventoryItem;
        }
        else
        {
            inventoryList.RemoveAt(itemPos);
        }
    }
    public ItemsDetails GetItemsDetails(int itemCode)
    {
        ItemsDetails itemsDetails = new ItemsDetails();
        if (itemDetailDictionary.TryGetValue(itemCode, out itemsDetails))
        {
            return itemsDetails;
        }
        else
        {
            return null;
        }
    }
    public string GetItemDescription(ItemType itemType)
    {
        string itemDescription = null;
        switch(itemType)
        {
            case ItemType.Hoeing_tool:
                itemDescription = Settings.HoeingTool;
                break;
            case ItemType.Watering_tool:
                itemDescription = Settings.WateringTool;
                break;
        }
        return itemDescription;
    }
    public void RemoveItem(InventoryLocation inventoryLocation,int itemCode)
    {
        List<InventoryItem> inventoryToRemoveList = inventoryLists[(int)inventoryLocation];
        int itemPos = FindItemInList(inventoryLocation, itemCode);
        if(itemPos != -1)
        {
            RemoveAtPos(inventoryToRemoveList, itemCode, itemPos);
        }
        EventHandler.CallInventoryUpdateEvents(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }
    public void SwapItems(InventoryLocation inventoryLocation,int fromItem,int toItem)
    {
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count
            && fromItem >= 0 && toItem >= 0 && fromItem != toItem)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];
            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;
            EventHandler.CallInventoryUpdateEvents(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }
    }
    private int GetSecletedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectInventoryItem[(int)inventoryLocation];
    }
    public ItemsDetails GetSeclectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSecletedInventoryItem(inventoryLocation);
        if(itemCode == -1)
        {
            return null;
        }
        else
        {
            return GetItemsDetails(itemCode);
        }
    }
}
