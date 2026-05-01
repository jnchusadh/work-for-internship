using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GridCursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;
    private bool cursorPositionIsValid = false;
    public bool CursorPositionIsValid
    {
        get
        {
            return cursorPositionIsValid;
        }
        set
        {
            cursorPositionIsValid = value;
        }
    }
    private int itemUseGridRadius = 0;
    public int ItemUseGridRadius
    {
        get
        {
            return itemUseGridRadius;
        }
        set
        {
            itemUseGridRadius = value;
        }
    }
    private ItemType selectedItemType;
    public ItemType SelectedItemType
    {
        get
        {
            return selectedItemType;
        }
        set
        {
            selectedItemType = value;
        }
    }
    private bool cursorIsEnable = false;
    public bool CursorIsEnable
    {
        get
        {
            return cursorIsEnable;
        }
        set
        {
            cursorIsEnable = value;
        }
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }
    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }
    private void Update()
    {
        if (cursorIsEnable)
        {
            DisplayCursor();
        }   
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)
        {
            Vector3Int gridPosition = GetGridPostionForCursor();
            Vector3Int playerGridPosition = GetGridPostionForPlayer();
            SetCursorValidity(gridPosition, playerGridPosition);
            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);
            return gridPosition;
        }
        else
        {
            return Vector3Int.zero;
        }
    }
    public Vector3Int GetGridPostionForCursor()
    {
        // 使用射线投射获取地面上的精确点
        Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector3 groundWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f);
        Vector3Int gridPos = grid.WorldToCell(groundWorldPos);

        /*
        if (Input.GetMouseButton(0))
        {
            Debug.Log($"[GridCursor] 坐标转换详情 | " +
                     $"鼠标屏幕:{mouseScreenPos} | " +
                     $"世界坐标:{mouseWorldPos:F2} | " +
                     $"地面坐标:{groundWorldPos:F2} | " +
                     $"网格坐标:{gridPos} | " +
                     $"Grid.cellSize:{grid.cellSize}");
        }
        */

        return gridPos;
    }
    public Vector3Int GetGridPostionForPlayer()
    {
        Vector3Int playerGridPos = grid.WorldToCell(PlayerManager.Instance.transform.position);

        /*
        if (Input.GetMouseButton(0))
        {
            Debug.Log($"[GridCursor] 玩家位置 | " +
                     $"世界坐标:{PlayerManager.Instance.transform.position:F2} | " +
                     $"网格坐标:{playerGridPos}");
        }
        */

        return playerGridPos;
    }
    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPos)
    {
        Vector3 gridWorldPos = grid.CellToWorld(gridPos);
        Vector2 gridScreenPos = Camera.main.WorldToScreenPoint(gridWorldPos);
        return RectTransformUtility.PixelAdjustPoint(gridScreenPos, cursorRectTransform, canvas);
    }
    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        cursorIsEnable = true;
    }
    public void DisableCursor()
    {
        cursorImage.color = Color.clear;
        cursorIsEnable = false;
    }
    private void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }
    private void SetCursorValidity(Vector3Int cursorGridPos, Vector3Int playerGridPos)
    {
        SetCursorToValid();
        //检测是否为有效半径
        if(Mathf.Abs(cursorGridPos.x - playerGridPos.x)> ItemUseGridRadius||
            Mathf.Abs(cursorGridPos.y - playerGridPos.y) > ItemUseGridRadius)
        {
            // Debug.LogWarning($"[GridCursor] 光标无效：超出使用范围 | 光标位置:{cursorGridPos} 玩家位置:{playerGridPos} 半径:{ItemUseGridRadius}");
            SetCursorToInvalid();
            return;
        }
        ItemsDetails itemsDetails = InventoryManager.Instance.GetSeclectedInventoryItemDetails(InventoryLocation.player);
        if (itemsDetails == null)
        {
            // Debug.LogWarning("[GridCursor] 光标无效：未选中任何物品");
            SetCursorToInvalid();
            return;
        }
        //获取光标位置的土块信息
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridDetials(cursorGridPos.x, cursorGridPos.y);
        if (gridPropertyDetails != null)
        {
            //根据选中的物品栏物品和网格属性详情，确定光标的有效性
            switch (itemsDetails.itemType)
            {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails))
                    {
                        // Debug.LogWarning($"[GridCursor] 光标无效：种子无法在此处种植 | 位置:{cursorGridPos} canDropItem:{gridPropertyDetails.canDropItem}");
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))
                    {
                        // Debug.LogWarning($"[GridCursor] 光标无效：商品无法丢弃 | 位置:{cursorGridPos} canDropItem:{gridPropertyDetails.canDropItem}");
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Collecting_tool:
                    if (!IsCursorValidForTool(gridPropertyDetails, itemsDetails))
                    {
                        // Debug.LogWarning($"[GridCursor] 光标无效：工具无法使用 | 物品类型:{itemsDetails.itemType} 位置:{cursorGridPos} isDigglable:{gridPropertyDetails?.isDigglable} daysSinceDug:{gridPropertyDetails?.daysSinceDug}");
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.none:
                    break;
                case ItemType.count:
                    break;
            }
        }
        else
        {
            // Debug.LogWarning($"[GridCursor] 光标无效：网格属性为空 | 位置:({cursorGridPos.x},{cursorGridPos.y})");
            SetCursorToInvalid();
            return;
        }
    }

    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, ItemsDetails itemsDetails)
    {
        switch (itemsDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if(gridPropertyDetails.isDigglable&&gridPropertyDetails.daysSinceDug == -1)
                {
                    Vector3 cursorWorldPos = new Vector3(GetWorldPositionForCursor().x + 0.5f, GetWorldPositionForCursor().y + 0.5f,0f);
                    List<Items> itemsList = new List<Items>();
                    HelperMethods.GetComponentsAtBoxLocation<Items>(out itemsList, cursorWorldPos, Settings.cursorSize, 0f);
                    //查找附近有无耕种过的土块
                    bool foundReaoable = false;
                    if (foundReaoable)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            case ItemType.Watering_tool:
                if(gridPropertyDetails.daysSinceDug>-1&&gridPropertyDetails.daysSinceWatered == -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case ItemType.Collecting_tool:
                if(gridPropertyDetails.seedItemCode != -1)
                {
                    Vector3 cursorWorldPos = grid.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
                    Vector3 checkPos = new Vector3(cursorWorldPos.x + 0.5f, cursorWorldPos.y + 0.5f, 0f);
                    
                    Collider2D[] colliders = Physics2D.OverlapPointAll(checkPos);
                    foreach (Collider2D collider in colliders)
                    {
                        Parsnip crop = collider.gameObject.GetComponentInParent<Parsnip>();
                        if (crop != null && crop.IsMature())
                        {
                            return true;
                        }
                        
                        crop = collider.gameObject.GetComponentInChildren<Parsnip>();
                        if (crop != null && crop.IsMature())
                        {
                            return true;
                        }
                    }
                }
                return false;
            default:
                return false;
        }  
    }

    public Vector3 GetWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPostionForCursor());
    }

    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }

    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }

    private void SetCursorToInvalid()
    {
        CursorPositionIsValid = false;
        cursorImage.sprite = redCursorSprite;
        cursorImage.color = Color.white;
    }

    private void SetCursorToValid()
    {
        CursorPositionIsValid = true;
        cursorImage.sprite = greenCursorSprite;
        cursorImage.color = Color.white;
    }
}
