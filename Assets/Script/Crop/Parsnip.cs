using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parsnip : MonoBehaviour
{

    [SerializeField] private int totalStages = 5;
    [SerializeField] private float[] daysPerStage = new float[] { 1f, 1f, 1f, 1f, 2f };
   
    [SerializeField] private GameObject[] growthStages;
    [SerializeField] private GameObject harvestItemPrefab;
    
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    
    [SerializeField] private float groundCheckRadius = 0.3f;

    [SerializeField] private int currentStage = 0;
    [SerializeField] private bool isMature = false;
    [SerializeField] private bool canGrow = false;
    
    private GameObject currentGrowthObject;
    private Collider2D cropCollider;
    private Vector3Int gridPosition;
    private GridPropertyDetails gridDetails;
    private int lastCheckedGrowthDays = -1;
    
    void Start()
    {
        cropCollider = GetComponent<Collider2D>();
        
        gridPosition = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
        
        UpdateGridDetailsFromManager();
        
        if (cropCollider != null)
        {
            cropCollider.enabled = false;
        }
        
        // 种子阶段（stage 0）：显示种子，不创建子对象
        SpriteRenderer seedSpriteRenderer = GetComponent<SpriteRenderer>();
        if (seedSpriteRenderer != null)
        {
            seedSpriteRenderer.enabled = true; // 显示种子
        }
        
        currentStage = 0;
        
        canGrow = CheckIfGroundIsTilled();
        
        if (gridDetails != null && gridDetails.growthDays > 0)
        {
            RestoreCropStage(gridDetails.growthDays);
        }
    }
    
    void OnDisable()
    {
        // 当对象被回收到对象池时，清理生长阶段对象
        if (currentGrowthObject != null)
        {
            ReturnGrowthObjectToPool();
        }
    }
    
    private void UpdateGridDetailsFromManager()
    {
        gridDetails = GridPropertiesManager.Instance.GetGridDetials(gridPosition.x, gridPosition.y);
    }
    
    private void RestoreCropStage(int savedGrowthDays)
    {
        int totalDays = 0;
        for (int i = 0; i < daysPerStage.Length; i++)
        {
            totalDays += (int)daysPerStage[i];
            if (savedGrowthDays >= totalDays)
            {
                currentStage = i + 1;
            }
            else
            {
                break;
            }
        }
        
        if (currentStage >= totalStages)
        {
            currentStage = totalStages - 1;
            ReachMatureStage();
        }
        else
        {
            // 如果阶段 >= 1，隐藏种子；如果阶段 == 0，显示种子
            SpriteRenderer seedSpriteRenderer = GetComponent<SpriteRenderer>();
            if (seedSpriteRenderer != null)
            {
                seedSpriteRenderer.enabled = (currentStage == 0);
            }
            
            if (currentGrowthObject != null)
            {
                ReturnGrowthObjectToPool();
            }
            
            if (currentStage < growthStages.Length && growthStages[currentStage] != null)
            {
                currentGrowthObject = PoolManager.Instance.ReuseObject(growthStages[currentStage], transform.position, Quaternion.identity);
                if (currentGrowthObject != null)
                {
                    currentGrowthObject.transform.SetParent(transform);
                }
            }
            
            
            if (currentStage >= totalStages - 1)
            {
                if (cropCollider != null)
                {
                    cropCollider.enabled = true;
                }
                isMature = true;
                canGrow = false;
            }
        }
    }
    
    void Update()
    {
        if (!canGrow || isMature)
        {
            return;
        }
        
        if (gridDetails != null && gridDetails.growthDays != lastCheckedGrowthDays)
        {
            lastCheckedGrowthDays = gridDetails.growthDays;
            
            if (currentStage < totalStages - 1)
            {
                int currentStageIndex = Mathf.Min(currentStage, daysPerStage.Length - 1);
                int daysNeededForThisStage = (int)daysPerStage[currentStageIndex];
                
                if (gridDetails.growthDays >= daysNeededForThisStage)
                {
                    AdvanceToNextStage();
                }
            }
        }
    }
    
    private void AdvanceToNextStage()
    {
        currentStage++;
        
        if (currentStage >= totalStages)
        {
            ReachMatureStage();
            return;
        }
        
        // 阶段 1：隐藏种子，显示阶段 1 对象
        if (currentStage == 1)
        {
            SpriteRenderer seedSpriteRenderer = GetComponent<SpriteRenderer>();
            if (seedSpriteRenderer != null)
            {
                seedSpriteRenderer.enabled = false; // 隐藏种子
            }
        }
        
        // 回收旧的生长对象
        if (currentGrowthObject != null)
        {
            ReturnGrowthObjectToPool();
        }
        
        // 创建新的生长阶段对象（从阶段 1 开始）
        if (currentStage < growthStages.Length && growthStages[currentStage] != null)
        {
            currentGrowthObject = PoolManager.Instance.ReuseObject(growthStages[currentStage], transform.position, Quaternion.identity);
            if (currentGrowthObject != null)
            {
                currentGrowthObject.transform.SetParent(transform);
            }
        }
        
        if (currentStage >= totalStages - 1)
        {
            if (cropCollider != null)
            {
                cropCollider.enabled = true;
            }
        }
    }
    
    private void ReturnGrowthObjectToPool()
    {
        if (currentGrowthObject != null)
        {
            // 使用当前生长对象查找对应的预制体
            GameObject prefabToReturn = null;
            foreach (var stage in growthStages)
            {
                if (stage != null && currentGrowthObject.name.StartsWith(stage.name.Replace("(Clone)", "").TrimEnd()))
                {
                    prefabToReturn = stage;
                    break;
                }
            }
            
            if (prefabToReturn != null)
            {
                PoolManager.Instance.ReturnToPool(prefabToReturn, currentGrowthObject);
            }
            else
            {
                Debug.LogWarning($"[Parsnip] Could not find prefab for growth object: {currentGrowthObject.name}");
            }
            
            currentGrowthObject = null;
        }
    }
    
    private void ReachMatureStage()
    {
        isMature = true;
        canGrow = false;
    }
    
    private bool CheckIfGroundIsTilled()
    {
        if (gridDetails != null)
        {
            return gridDetails.daysSinceDug >= 0;
        }
        
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPosition, groundCheckRadius);
        
        foreach (Collider2D col in colliders)
        {
            GridProperty gridProperty = col.GetComponent<GridProperty>();
            if (gridProperty != null)
            {
                Vector3Int gridPos = new Vector3Int(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, 0);
                GridPropertyDetails groundDetails = GridPropertiesManager.Instance.GetGridDetials(gridPos.x, gridPos.y);
                if (groundDetails != null && groundDetails.daysSinceDug >= 0)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public bool IsMature()
    {
        if (isMature)
        {
            return true;
        }
        
        UpdateGridDetailsFromManager();
        
        if (gridDetails != null && gridDetails.growthDays >= 0)
        {
            int totalDaysNeeded = 0;
            for (int i = 0; i < daysPerStage.Length; i++)
            {
                totalDaysNeeded += (int)daysPerStage[i];
            }
            
            if (gridDetails.growthDays >= totalDaysNeeded)
            {
                isMature = true;
                canGrow = false;
                
                if (cropCollider != null)
                {
                    cropCollider.enabled = true;
                }
                
                return true;
            }
        }
        
        return false;
    }
    
    public int GetCurrentStage()
    {
        return currentStage;
    }
    
    public void HarvestCrop()
    {
        if (currentGrowthObject != null)
        {
            ReturnGrowthObjectToPool();
        }

        GameObject seedPrefab = GetSeedPrefabByCode(gridDetails.seedItemCode);
        if (seedPrefab != null)
        {
            PoolManager.Instance.ReturnToPool(seedPrefab, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ProcessToolAction(GridPropertyDetails gridDetails, ItemsDetails itemDetails)
    {
        UpdateGridDetailsFromManager();

        if (gridDetails != null && this.gridDetails != null && this.gridDetails.seedItemCode != -1)
        {
            HarvestCrop(this.gridDetails);
        }
    }

    private void HarvestCrop(GridPropertyDetails gridDetails)
    {
        ItemsDetails cropItemDetails = InventoryManager.Instance.GetItemsDetails(gridDetails.seedItemCode);

        gridDetails.seedItemCode = -1;
        gridDetails.growthDays = -1;

        GridPropertiesManager.Instance.SetGridDetials(gridDetails.gridX, gridDetails.gridY, gridDetails);

        HarvestActions(cropItemDetails, gridDetails);

        if (currentGrowthObject != null)
        {
            ReturnGrowthObjectToPool();
        }

        GameObject seedPrefab = GetSeedPrefabByCode(this.gridDetails.seedItemCode);
        if (seedPrefab != null)
        {
            PoolManager.Instance.ReturnToPool(seedPrefab, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HarvestActions(ItemsDetails cropItemDetails, GridPropertyDetails gridDetails)
    {
        if (cropItemDetails != null)
        {
            SpawnHarvestItems(cropItemDetails, gridDetails);
        }
    }

    private void SpawnHarvestItems(ItemsDetails cropItemDetails, GridPropertyDetails gridDetails)
    {
        float randomX = Random.Range(-0.5f, 0.5f);
        float randomY = Random.Range(-0.5f, 0.5f);
        Vector3 spawnPosition = new Vector3(gridDetails.gridX + 0.5f + randomX, gridDetails.gridY + 0.5f + randomY, 0f);

        GameObject harvestItemPrefab = GetHarvestItemPrefabByItemCode(cropItemDetails.itemCode);
        if (harvestItemPrefab != null)
        {
            PoolManager.Instance.ReuseObject(harvestItemPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private GameObject GetHarvestItemPrefabByItemCode(int itemCode)
    {
        if (harvestItemPrefab != null)
        {
            return harvestItemPrefab;
        }

        Debug.LogWarning($"[Parsnip] 未设置收获物品预制体！请 Inspector 中配置 Harvest Item Prefab 字段");
        return null;
    }
    
    private GameObject GetSeedPrefabByCode(int seedCode)
    {
        switch (seedCode)
        {
            case 6:
            case 101:
                return growthStages.Length > 0 ? growthStages[0] : null;
            default:
                return growthStages.Length > 0 ? growthStages[0] : null;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
    }
}
