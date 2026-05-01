using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    private float xInput;
    private float yInput;
    private float playerSpeed;
    private bool isWalking;
    private bool isRunning;
    private bool isIdle;
    private bool isCarrying;
    private bool isUsingToolRight;
    private bool isUsingToolLeft;
    private bool isUsingToolUp;
    private bool isUsingToolDown;
    private bool isLiftingToolRight;
    private bool isLiftingToolLeft;
    private bool isLiftingToolUp;
    private bool isLiftingToolDown;
    private bool isPickingRight;
    private bool isPickingLeft;
    private bool isPickingUp;
    private bool isPickingDown;
    private bool playerCanMove = true;
    private bool playerToolUseDisabled = false;

    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds pickingAnimationPause;
    private WaitForSeconds afterPickingAnimationPause;


    private GridCursor gridCursor;
    private Cursor cursor;
    private ToolUseManager toolUseManager;

    private CharaAttribute armsCharacterAttribute;
    private CharaAttribute toolsCharacterAttribute;
    private Direction playerDirection;
    private ToolEffect toolEffect = ToolEffect.none;
    private Rigidbody2D rb;
    private Animator anim;
    private AnimatorOverrideControls overrideControls;
    private List<CharaAttribute> charaAttributeCustomisationList;
    
    [SerializeField] private SpriteRenderer equipSpriteRenderer = null;
    
    [SerializeField] private GameObject parsnipSeedPrefab;
    
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        overrideControls = GetComponentInChildren<AnimatorOverrideControls>();
        armsCharacterAttribute = new CharaAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);
        toolsCharacterAttribute = new CharaAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.hoe);
        charaAttributeCustomisationList = new List<CharaAttribute>();
    }
    
    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        toolUseManager = GetComponent<ToolUseManager>();
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        pickingAnimationPause = new WaitForSeconds(Settings.pickingAnimationPause);
        afterPickingAnimationPause = new WaitForSeconds(Settings.afterPickingAnimationPause);

        EventHandler.AfterSceneLoadEvent += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        EventHandler.AfterSceneLoadEvent -= OnSceneLoaded;
    }

    private void OnSceneLoaded()
    {
        EnablePlayerMove();
        playerToolUseDisabled = false;

        StartCoroutine(AutoSelectFirstItemDelayed());
    }

    private IEnumerator AutoSelectFirstItemDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        // Debug.Log("[PlayerManager] 开始自动选中第一个物品...");

        List<InventoryItem> playerInventory = InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player];
        if (playerInventory != null && playerInventory.Count > 0)
        {
            int firstItemCode = playerInventory[0].itemCode;
            ItemsDetails firstItemDetails = InventoryManager.Instance.GetItemsDetails(firstItemCode);

            // Debug.Log($"[PlayerManager] 找到第一个物品: itemCode={firstItemCode}, itemType={firstItemDetails?.itemType}, gridRadius={firstItemDetails?.itemUsedGridRadius}");

            if (firstItemDetails != null)
            {
                InventoryManager.Instance.SetSelectInventoryItem(InventoryLocation.player, firstItemCode);
                // Debug.Log($"[PlayerManager] 已设置选中物品: {firstItemDetails.itemDescription}");

                if (firstItemDetails.canBeCarried)
                {
                    ShowCarriedItem(firstItemCode);
                    // Debug.Log("[PlayerManager] 显示手持物品");
                }

                GridCursor gridCursor = FindObjectOfType<GridCursor>();
                if (gridCursor != null)
                {
                    gridCursor.ItemUseGridRadius = firstItemDetails.itemUsedGridRadius;
                    gridCursor.SelectedItemType = firstItemDetails.itemType;

                    if (firstItemDetails.itemUsedGridRadius > 0)
                    {
                        gridCursor.EnableCursor();
                        // Debug.Log($"[PlayerManager] 光标已启用 | 半径:{firstItemDetails.itemUsedGridRadius} 类型:{firstItemDetails.itemType}");
                    }
                    else
                    {
                        // Debug.Log($"[PlayerManager] 物品gridRadius=0，光标保持禁用 | 类型:{firstItemDetails.itemType}");
                    }
                }
                else
                {
                    // Debug.LogError("[PlayerManager] 未找到GridCursor！");
                }

                EventHandler.CallInventoryUpdateEvents(InventoryLocation.player, playerInventory);
            }
            else
            {
                // Debug.LogError($"[PlayerManager] 无法获取物品详情: itemCode={firstItemCode}");
            }
        }
        else
        {
            // Debug.LogWarning("[PlayerManager] 物品栏为空或null！");
        }
    }

    private void Update()
    {
        TestSomeThing();
        if (playerCanMove)
        {
            ResetPlayerAnimation();
            PlayerMovementSelect();
            PlayerClickInput();
            PlayerWalkingOrRunning();
            if(isCarrying)
            {
                UpdateCarriedItemAnimation();
            }
            EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying,
                toolEffect, isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                isPickingRight, isPickingLeft, isPickingUp, isPickingDown);
        }
    }
    
    private void FixedUpdate()
    {
        PlayerMove();
    }
    
    private void ResetPlayerMovement()
    {
        xInput = 0f;
        yInput = 0f;
        isRunning = false;
        isWalking = false;
        isIdle = true;
    }
    
    public void DisableAndResetPlayerMovement()
    {
        DisablePlayerMove();
        ResetPlayerMovement();
        EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying,
            toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown);
    }
    
    public void DisablePlayerMove()
    {
        playerCanMove = false;
    }
    
    public void EnablePlayerMove()
    {
        playerCanMove = true;
    }
    
    private void PlayerWalkingOrRunning()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = false;
            isIdle = false;
            isWalking = true;
            playerSpeed = Settings.walkSpeed;
        }
        else
        {
            isRunning = true;
            isIdle = false;
            isWalking = false;
            playerSpeed = Settings.runSpeed;
        }
    }

    private void PlayerMove()
    {
        Vector2 move = new Vector2(xInput * playerSpeed * Time.deltaTime, yInput * playerSpeed * Time.deltaTime);
        rb.MovePosition(rb.position + move);
    }

    private void PlayerMovementSelect()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        if (xInput != 0 && yInput != 0)
        {
            xInput = xInput * 0.71f;
            yInput = yInput * 0.71f;
        }
        else if (xInput != 0 || yInput != 0)
        {
            isIdle = false;
            isWalking = false;
            isRunning = true;
            if (xInput < 0)
            {
                playerDirection = Direction.left;
            }
            else if (xInput > 0)
            {
                playerDirection = Direction.right;
            }
            if (yInput < 0)
            {
                playerDirection = Direction.down;
            }
            else if (yInput > 0)
            {
                playerDirection = Direction.up;
            }
            else if (xInput == 0 && yInput == 0)
            {
                isIdle = true;
                isWalking = false;
                isRunning = false;
            }
        }
    }

    private void ResetPlayerAnimation()
    {
        isUsingToolRight = false;
        isUsingToolLeft = false;
        isUsingToolUp = false;
        isUsingToolDown = false;
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;
        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;
        toolEffect = ToolEffect.none;
    }
    
    public void ClearCarriedItem()
    {
        equipSpriteRenderer.sprite = null;
        equipSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        if (charaAttributeCustomisationList != null)
        {
            armsCharacterAttribute.partVariantType = PartVariantType.none;
            charaAttributeCustomisationList.Clear();
            charaAttributeCustomisationList.Add(armsCharacterAttribute);
            overrideControls.ApplyCharacterCustomisationPatameters(charaAttributeCustomisationList);
        }

        isCarrying = false;
    }
    
    public void ShowCarriedItem(int itemCode)
    {
        ItemsDetails itemsDetails = InventoryManager.Instance.GetItemsDetails(itemCode);
        if (itemsDetails != null)
        {
            equipSpriteRenderer.sprite = itemsDetails.itemsSprite;
            equipSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            if (charaAttributeCustomisationList != null)
            {
                armsCharacterAttribute.partVariantType = PartVariantType.carry;
                charaAttributeCustomisationList.Clear();
                charaAttributeCustomisationList.Add(armsCharacterAttribute); 
                if(overrideControls != null)
                {
                    overrideControls.ApplyCharacterCustomisationPatameters(charaAttributeCustomisationList);
                }
            }
            isCarrying = true;
        }
    }
    
    public Vector3 GetPosition()
    {
        return Camera.main.WorldToViewportPoint(transform.position);
    }
    
    public void UpdateCarriedItemAnimation()
    {
        if (charaAttributeCustomisationList != null && overrideControls != null)
        {
            armsCharacterAttribute.partVariantType = PartVariantType.carry;
            charaAttributeCustomisationList.Clear();
            charaAttributeCustomisationList.Add(armsCharacterAttribute);
            overrideControls.ApplyCharacterCustomisationPatameters(charaAttributeCustomisationList);
        }
    }
    public void TestSomeThing()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 切换到 Field 场景
            SceneChangeManager.Instance.FadeAndLoadScene(SceneName.Scene2_Field.ToString(), transform.position);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 切换到 Farm 场景
            SceneChangeManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 切换到 Cabin 场景
            SceneChangeManager.Instance.FadeAndLoadScene(SceneName.Scene3_Cabin.ToString(), transform.position);
        }
    }
    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (gridCursor.CursorIsEnable)
                {
                Vector3Int currentGridPosition = gridCursor.GetGridPostionForCursor();

                Vector3Int playerGridPosition = gridCursor.GetGridPostionForPlayer();
                    ProcessPlayerClickInput(currentGridPosition, playerGridPosition);
                }
            }
        }
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetPlayerMovement();

        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);
        //获取光标位置的网格属性
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridDetials(cursorGridPosition.x, cursorGridPosition.y);

        ItemsDetails itemsDetails = InventoryManager.Instance.GetSeclectedInventoryItemDetails(InventoryLocation.player);
        if (itemsDetails != null)
        {
            switch (itemsDetails.itemType)
            {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(itemsDetails);
                    }
                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemsDetails);
                    }
                    break;
                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                        ProcessPlayerClickInputTool(gridPropertyDetails, itemsDetails, playerDirection);
                        break;
                case ItemType.Collecting_tool:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputTool(gridPropertyDetails, itemsDetails, playerDirection);
                    }
                    break;
                case ItemType.none:
                    break;
                case ItemType.count:
                    break;
                default:
                    break;
            }
        }
    }

    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemsDetails itemsDetails, Vector3Int playerDirection)
    {
        if (!gridCursor.CursorPositionIsValid || gridPropertyDetails == null)
        {
            return;
        }

        DisablePlayerMove();
        playerToolUseDisabled = true;
        switch (itemsDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridPropertyDetails.isDigglable && gridPropertyDetails.daysSinceDug == -1)
                {
                    toolUseManager.HoeGround(playerDirection, gridPropertyDetails, () =>
                    {
                        EnablePlayerMove();
                        playerToolUseDisabled = false;
                    });
                }
                else
                {
                    EnablePlayerMove();
                    playerToolUseDisabled = false;
                }
                break;
            case ItemType.Watering_tool:
                if (gridPropertyDetails.daysSinceDug >= 0)
                {
                    toolUseManager.WaterGround(playerDirection, gridPropertyDetails, () =>
                    {
                        EnablePlayerMove();
                        playerToolUseDisabled = false;
                    });
                }
                else
                {
                    EnablePlayerMove();
                    playerToolUseDisabled = false;
                }
                break;
            case ItemType.Collecting_tool:
                if (gridCursor.CursorPositionIsValid && gridPropertyDetails != null)
                {
                    ProcessCropInPlayerDirection(playerDirection);
                }
                else
                {
                    EnablePlayerMove();
                    playerToolUseDisabled = false;
                }
                break;
            default:
                    EnablePlayerMove();
                    playerToolUseDisabled = false;
                    break;
        }
    }

    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPos,Vector3Int playerGridPos)
    {
        if (cursorGridPos.x > playerGridPos.x)
        {
            return Vector3Int.right;
        }
        else if (cursorGridPos.x < playerGridPos.x)
        {
            return Vector3Int.left;
        }
        else if (cursorGridPos.y > playerGridPos.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }
    private void ProcessPlayerClickInputCommodity(ItemsDetails itemsDetails)
    {
        if (itemsDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void ProcessPlayerClickInputSeed(ItemsDetails itemsDetails)
    {
        if (itemsDetails == null)
        {
            return;
        }
        
        if (!gridCursor.CursorPositionIsValid)
        {
            return;
        }
        
        Vector3Int cursorGridPosition = gridCursor.GetGridPostionForCursor();
        Vector3 cursorWorldPos = gridCursor.GetWorldPositionForCursor();
        
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridDetials(
            cursorGridPosition.x, cursorGridPosition.y);
        
        if (gridPropertyDetails == null)
        {
            return;
        }
        
        if (gridPropertyDetails.daysSinceDug < 0)
        {
            return;
        }
        
        if (gridPropertyDetails.seedItemCode != -1)
        {
            return;
        }
        
        PlantSeed(cursorGridPosition, itemsDetails.itemCode, gridPropertyDetails);
    }

    private void PlantSeed(Vector3Int gridPosition, int seedItemCode, GridPropertyDetails gridDetails)
    {
        gridDetails.seedItemCode = seedItemCode;
        gridDetails.growthDays = 0;
        GridPropertiesManager.Instance.SetGridDetials(gridPosition.x, gridPosition.y, gridDetails);
        
        Vector3 worldPosition = new Vector3((float)gridPosition.x + 0.5f, (float)gridPosition.y, 0f);
        
        GameObject seedPrefab = GetSeedPrefabByCode(seedItemCode);
        if (seedPrefab != null)
        {
            PoolManager.Instance.ReuseObject(seedPrefab, worldPosition, Quaternion.identity);
            
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, seedItemCode);
            EventHandler.CallInventoryUpdateEvents(InventoryLocation.player, 
                InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player]);
        }
    }

    private GameObject GetSeedPrefabByCode(int seedCode)
    {
        switch (seedCode)
        {
            case 6:
            case 101:
                if (parsnipSeedPrefab != null)
                {
                    return parsnipSeedPrefab;
                }
                else
                {
                    return null;
                }
            default:
                return null;
        }
    }
    public Vector3 GetPlayerCentrePosition()
    {
        return Camera.main.WorldToViewportPoint(transform.position);
    }

    public void SetUsingToolRight(bool value) => isUsingToolRight = value;
    public void SetUsingToolLeft(bool value) => isUsingToolLeft = value;
    public void SetUsingToolUp(bool value) => isUsingToolUp = value;
    public void SetUsingToolDown(bool value) => isUsingToolDown = value;
    public void SetLiftingToolRight(bool value) => isLiftingToolRight = value;
    public void SetLiftingToolLeft(bool value) => isLiftingToolLeft = value;
    public void SetLiftingToolUp(bool value) => isLiftingToolUp = value;
    public void SetLiftingToolDown(bool value) => isLiftingToolDown = value;
    public void SetPickingRight(bool value) => isPickingRight = value;
    public void SetPickingLeft(bool value) => isPickingLeft = value;
    public void SetPickingUp(bool value) => isPickingUp = value;
    public void SetPickingDown(bool value) => isPickingDown = value;

    private void ProcessCropInPlayerDirection(Vector3Int playerDirection)
    {
        StartCoroutine(ProcessCropInPlayerDirectionRoutine(playerDirection));
    }

    private IEnumerator ProcessCropInPlayerDirectionRoutine(Vector3Int playerDirection)
    {
        DisablePlayerMove();
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection);

        yield return pickingAnimationPause;

        yield return afterPickingAnimationPause;

        EnablePlayerMove();
        playerToolUseDisabled = false;
    }

    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection)
    {
        if (playerDirection == Vector3Int.right)
        {
            isPickingRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isPickingLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isPickingUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isPickingDown = true;
        }

        Vector3Int cursorGridPosition = gridCursor.GetGridPostionForCursor();
        GridPropertyDetails gridDetails = GridPropertiesManager.Instance.GetGridDetials(cursorGridPosition.x, cursorGridPosition.y);
        ItemsDetails itemDetails = InventoryManager.Instance.GetSeclectedInventoryItemDetails(InventoryLocation.player);
        Parsnip crop = GridPropertiesManager.Instance.GetCropObjectAtGridPosition(cursorGridPosition.x, cursorGridPosition.y);

        if (crop != null && gridDetails != null)
        {
            crop.ProcessToolAction(gridDetails, itemDetails);
        }
    }
}
