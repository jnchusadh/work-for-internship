using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBar : MonoBehaviour
{
    [SerializeField] private Sprite blank16x16sprites = null;
    [SerializeField] private InventorySlot[] inventorySlots = null;
    [HideInInspector] public GameObject textDescriptionBox;
    public GameObject inventoryDraggedItem;


    private RectTransform rectTransform;
    private bool isInventoryBottom = true;
    public bool IsInventoryBottom
    {
        get
        {
            return isInventoryBottom;
        }
        set
        {
            isInventoryBottom = value;
        }
    }
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        EventHandler.InventoryUpdateEvents += InventoryUpdate;
    }
    public void SetSelectedHighlight()
    {
        if (inventorySlots.Length > 0)
        {
              for(int i = 0; i < inventorySlots.Length; i++)
            {
                SetSelectedHighlight(i);
            }
        }
    }
    public void SetSelectedHighlight(int itemPos)
    {
        if (inventorySlots.Length > 0 && inventorySlots[itemPos].itemsDetails != null)
        {
            if (inventorySlots[itemPos].isSelected)
            {
                inventorySlots[itemPos].inventoryHighlight.color = new Color(1f, 1f, 1f, 1f);
                InventoryManager.Instance.SetSelectInventoryItem(InventoryLocation.player,inventorySlots[itemPos].itemsDetails.itemCode);
            }
        }
    }
    public void ClearSelectedHighlight()
    {
        if (inventorySlots.Length > 0)
        {
            for(int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].isSelected)
                {
                    inventorySlots[i].isSelected = false;
                    inventorySlots[i].inventoryHighlight.color = new Color(0f, 0f, 0f,0f);
                    InventoryManager.Instance.ClearSelectInventoryItem(InventoryLocation.player);
                }
            }
        }
    }
    private void ClearInventorySlot()
    {
        if (inventorySlots.Length > 0)
        {
            for(int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i].inentorySlotImage.sprite = blank16x16sprites;
                inventorySlots[i].textMeshProUGUI.text = "";
                inventorySlots[i].itemsDetails = null;
                inventorySlots[i].itemQuanity = 0;
                inventorySlots[i].isSelected = false;
                inventorySlots[i].inventoryHighlight.color = new Color(0f, 0f, 0f,0f);
            }
        }
    }
    private void InventoryUpdate(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
    {
        if(inventoryLocation == InventoryLocation.player)
        {
            //Debug.Log("InventoryUpdate: inventoryList.Count={inventoryList.Count}, inventorySlots.Length={inventorySlots.Length}");
            
            // 不再全部清空，而是逐个更新槽位
            if (inventorySlots.Length > 0)
            {
                // 更新现有物品的槽位
                for(int i = 0; i < inventorySlots.Length; i++)
                {
                    if (i < inventoryList.Count)
                    {
                        // 有物品的槽位
                        int itemCode = inventoryList[i].itemCode;
                        ItemsDetails itemsDetails = InventoryManager.Instance.GetItemsDetails(itemCode);
                        if(itemsDetails != null)
                        {
                            // 如果物品发生变化才更新
                            if(inventorySlots[i].itemsDetails == null || 
                               inventorySlots[i].itemsDetails.itemCode != itemCode ||
                               inventorySlots[i].itemQuanity != inventoryList[i].itemQuanity)
                            {
                                //Debug.Log("更新槽位{i}: {itemsDetails.itemName} x{inventoryList[i].itemQuanity}");
                                inventorySlots[i].inentorySlotImage.sprite = itemsDetails.itemsSprite;
                                inventorySlots[i].textMeshProUGUI.text = inventoryList[i].itemQuanity.ToString();
                                inventorySlots[i].itemsDetails = itemsDetails;
                                inventorySlots[i].itemQuanity = inventoryList[i].itemQuanity;
                            }
                            // 恢复选中状态 - 检查当前选中的物品代码
                            int selectedCode = InventoryManager.Instance.selectInventoryItem[(int)InventoryLocation.player];
                            if(selectedCode == itemCode)
                            {
                                inventorySlots[i].isSelected = true;
                                inventorySlots[i].inventoryHighlight.color = new Color(1f, 1f, 1f, 1f);
                            }
                            else
                            {
                                inventorySlots[i].isSelected = false;
                                inventorySlots[i].inventoryHighlight.color = new Color(0f, 0f, 0f,0f);
                            }
                        }
                    }
                    else
                    {
                        // 超出物品数量的槽位，只有当槽位原本有内容时才清空
                        if(inventorySlots[i].itemsDetails != null)
                        {
                            inventorySlots[i].inentorySlotImage.sprite = blank16x16sprites;
                            inventorySlots[i].textMeshProUGUI.text = "";
                            inventorySlots[i].itemsDetails = null;
                            inventorySlots[i].itemQuanity = 0;
                            inventorySlots[i].isSelected = false;
                            inventorySlots[i].inventoryHighlight.color = new Color(0f, 0f, 0f,0f);
                        }
                    }
                }
            }
        }
    }
    private void Update()
    {
        SwitchBarTopOrBottom();
    }

    private void SwitchBarTopOrBottom()
    {
        if(PlayerManager.Instance == null)
        {
            return;
        }
        Vector3 playerPos = PlayerManager.Instance.GetPosition();
        if(playerPos.y > 0.3f && isInventoryBottom == false)
        {
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 2.5f);
            isInventoryBottom = true;
        }
        else if(playerPos.y<=0.3f && isInventoryBottom == true)
        {
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);
            isInventoryBottom = false;
        }
    }
}
