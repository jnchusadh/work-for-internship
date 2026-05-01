using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class InventorySlot : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    private Canvas parentCanvas;
    private Transform parentItem;
    private GameObject draggerItem;
    private GridCursor gridCursor;
    private Cursor cursor;


    public Image inentorySlotImage;
    public Image inventoryHighlight;
    public TextMeshProUGUI textMeshProUGUI;


    [HideInInspector] public ItemsDetails itemsDetails;
    [HideInInspector] public int itemQuanity;
    [HideInInspector] public bool isSelected = false;



    [SerializeField] private InventoryBar inventoryBar = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;
    [SerializeField] private GameObject itemsPrafab = null;
    [SerializeField] private int slotNumber = 0;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoad;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoad;
    }
    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
    }
    private void ClearCursor()
    {
        gridCursor.DisableCursor();
        gridCursor.SelectedItemType = ItemType.none;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemsDetails != null)
        {
            PlayerManager.Instance.DisableAndResetPlayerMovement();
            draggerItem = Instantiate(inventoryBar.inventoryDraggedItem, inventoryBar.transform);
            Image draggerItemSprite = draggerItem.GetComponentInChildren<Image>();
            draggerItemSprite.sprite = inentorySlotImage.sprite;
            SetSelected();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(draggerItem != null)
        {
            draggerItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggerItem != null)
        {
            try
            {
                Destroy(draggerItem);
                if (eventData.pointerCurrentRaycast.gameObject != null&&eventData.pointerCurrentRaycast.
                    gameObject.GetComponent<InventorySlot>() != null)
                {
                    int toSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<InventorySlot>().slotNumber;
                    InventoryManager.Instance.SwapItems(InventoryLocation.player, slotNumber, toSlot);
                    ClearSelected();
                    PlayerManager.Instance.ClearCarriedItem();
                    DestroyTextBox();
                }
                else
                {
                    if (itemsDetails.canBeDropped)
                    {
                        DropSelectedItemAtMousePosition();
                    }
                    else
                    {
                        //不能丢弃的物品，清除选中状态和动画
                        ClearSelected();
                        PlayerManager.Instance.ClearCarriedItem();
                    }
                }
            }
            finally
            {
                //确保无论如何都启用玩家移动
                PlayerManager.Instance.EnablePlayerMove();
            }
        }
    }
    private void DropSelectedItemAtMousePosition()
    {
        if(itemsDetails != null&&isSelected)
        {
            //保存物品代码，防止后面被清除
            int itemCode = itemsDetails.itemCode;
            if (gridCursor.CursorPositionIsValid)
            {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint
                (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

          
                GameObject itemGameObject = Instantiate(itemsPrafab, worldPosition, Quaternion.identity, parentItem);
                Items item = itemGameObject.GetComponent<Items>();
                item.Items_code = itemCode;
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);

                //检查物品是否还存在
                if(InventoryManager.Instance.FindItemInList(InventoryLocation.player, itemCode) == -1)
                {
                    ClearSelected();
                    PlayerManager.Instance.ClearCarriedItem();
                }
                else
                {
                    PlayerManager.Instance.ShowCarriedItem(itemCode);
                }
            }
            else
            {
                // Debug.LogWarning("不能在此处丢弃物品gridPropertyDetails: {(gridPropertyDetails == null ? "null" : "not null")}, canDropItem: {gridPropertyDetails?.canDropItem}");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryBar == null || itemsDetails == null || parentCanvas == null || inventoryTextBoxPrefab == null)
        {
            return;
        }
        if (itemQuanity > 0)
        {
            inventoryBar.textDescriptionBox = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryBar.textDescriptionBox.transform.SetParent(parentCanvas.transform, false);
        }
        if (inventoryBar.textDescriptionBox != null)
        {
            InventoryTextBox inventoryTextBox = inventoryBar.textDescriptionBox.GetComponent<InventoryTextBox>();
            string itemTypeDescriotion = InventoryManager.Instance.GetItemDescription(itemsDetails.itemType);
            inventoryTextBox.SetText(itemsDetails.itemDescription,itemsDetails.itemType.ToString(),"",itemsDetails.itemLongDescription,"","");
            if (inventoryBar.IsInventoryBottom)
            {
                inventoryBar.textDescriptionBox.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.textDescriptionBox.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.textDescriptionBox.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.textDescriptionBox.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryBar.textDescriptionBox != null)
        {
            DestroyTextBox();
        }
    }

    private void DestroyTextBox()
    {
        if (inventoryBar.textDescriptionBox != null)
        {
            Destroy(inventoryBar.textDescriptionBox);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isSelected == true)
            {
                ClearSelected();
            }
            else
            {
                if (itemQuanity > 0)
                {
                    //点击物品时禁用玩家移动（点击移动）
                    PlayerManager.Instance.DisableAndResetPlayerMovement();
                    SetSelected();
                    //操作完成后启用玩家移动
                    PlayerManager.Instance.EnablePlayerMove();
                }
            }
        }
    }

    private void SetSelected()
    {
        if (inventoryBar != null)
        {
            inventoryBar.ClearSelectedHighlight();
            isSelected = true;
            inventoryBar.SetSelectedHighlight();
            gridCursor.ItemUseGridRadius = itemsDetails.itemUsedGridRadius;
                if (itemsDetails.itemUsedGridRadius > 0)
                {
                    gridCursor.EnableCursor();
                }
                else
                {
                    gridCursor.DisableCursor();
                }
            gridCursor.SelectedItemType = itemsDetails.itemType;
            InventoryManager.Instance.SetSelectInventoryItem(InventoryLocation.player,itemsDetails.itemCode);
            if(itemsDetails.canBeCarried == true)
            {
                PlayerManager.Instance.ShowCarriedItem(itemsDetails.itemCode);
            }
            else
            {
                PlayerManager.Instance.ClearCarriedItem();
            }
        }
    }

    private void ClearSelected()
    {
        if (inventoryBar != null)
        {
            inventoryBar.ClearSelectedHighlight();
            isSelected = false;
            InventoryManager.Instance.ClearSelectInventoryItem(InventoryLocation.player);
            //清除手持动画
            PlayerManager.Instance.ClearCarriedItem();
        }
    }
    //场景加载时获取物品父对象，防止场景切换时找不到物品父对象
    public void SceneLoad()
    {
        GameObject parentObj = GameObject.FindGameObjectWithTag(Tags.ItemParentTransform);
        if (parentObj != null)
        {
            parentItem = parentObj.transform;
        }
    }
}
