using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : Singleton<GridPropertiesManager>,ISaveable
{
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;
    private Grid grid;
    private Dictionary<string, GridPropertyDetails> gridPropertyDict;
    [SerializeField] private SO_GridProperty[] so_GridPropertiesArray = null;
    [SerializeField] private Tile[] dugGround = null;

    private string iSaveableUniqueID;
    public string ISaveableUniqueID
    {
        get
        {
            return iSaveableUniqueID;
        }
        set
        {
            iSaveableUniqueID = value;
        }
    }
    private GameObjectSave gameObjectSave;
    public GameObjectSave GameObjectSave
    {
        get
        {
            return gameObjectSave;
        }
        set
        {
            gameObjectSave = value;
        }
    }

    public string ISaveableUniqueId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    protected override void Awake()
    {
        base.Awake();
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }
    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterScemeLoaded;
        EventHandler.gameTimeDayEvent += OnDayChanged;
    }
    
    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterScemeLoaded;
        EventHandler.gameTimeDayEvent -= OnDayChanged;
    }
    
    private void OnDayChanged(int gameYear, Season gameSeason, int gameDay, string dayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        UpdateAllCropGrowthDays();
    }
    
    private void UpdateAllCropGrowthDays()
    {
        if (gridPropertyDict == null)
        {
            return;
        }
        
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDict)
        {
            GridPropertyDetails gridDetails = item.Value;
            
            if (gridDetails.seedItemCode != -1 && gridDetails.daysSinceDug >= 0)
            {
                gridDetails.growthDays++;
            }
        }
    }
    private void Start()
    {
        InitialiseGridProperties();
    }
    private void InitialiseGridProperties()
    {
        // 确保 gridPropertyDict 被初始化
        if(gridPropertyDict == null)
        {
            gridPropertyDict = new Dictionary<string, GridPropertyDetails>();
        }
        else
        {
            return;
        }
        
        // 检查数组是否为 null
        if(so_GridPropertiesArray == null || so_GridPropertiesArray.Length == 0)
        {
            Debug.LogError("so_GridPropertiesArray is null or empty!");
            return;
        }
        
        // Debug.Log(&"InitialiseGridProperties - 开始初始化，共有 {so_GridPropertiesArray.Length} 个 SO_GridProperty");
        
        foreach(SO_GridProperty so_GridProperty in so_GridPropertiesArray)
        {
            if(so_GridProperty == null)
            {
                Debug.LogWarning("Found null SO_GridProperty in array, skipping...");
                continue;
            }
            
            if(so_GridProperty.gridPropertyList == null)
            {
                Debug.LogWarning($"gridPropertyList is null for {so_GridProperty.sceneName}, skipping...");
                continue;
            }
           
            
            Dictionary<string, GridPropertyDetails> gridPropertyDict = new Dictionary<string, GridPropertyDetails>();
            foreach(GridProperty gridProperty in so_GridProperty.gridPropertyList)
            {
                if(gridProperty == null)
                {
                    //Debug.LogWarning("Found null GridProperty in list");
                    continue;
                }
                
                if(gridProperty.gridCoordinate == null)
                {
                    continue;
                }
                
                GridPropertyDetails gridPropertyDetails;
                gridPropertyDetails = GetGridDetials(gridProperty.gridCoordinate.x,gridProperty.gridCoordinate.y, gridPropertyDict);
                if(gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }
                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDigglable = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;
                    default:
                        break;
                }
                SetGridDetials(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y,gridPropertyDetails,gridPropertyDict);
            }
            SceneSave sceneSave = new SceneSave();
            if(so_GridProperty.sceneName.ToString() == SceneChangeManager.Instance.startSceneName.ToString())
            {
                this.gridPropertyDict = gridPropertyDict;
            }
            GameObjectSave.sceneData.Add(so_GridProperty.sceneName.ToString(), sceneSave);
        }
    }
    private void ClearDisplayGroundDecorations()
    {
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles(); 
    }
    private void ClearDisplayGroundDetails()
    {
        ClearDisplayGroundDecorations();
    }
    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        List<Vector2Int> tilesToUpdate = new List<Vector2Int>();
        
        tilesToUpdate.Add(new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY));
        tilesToUpdate.Add(new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1));
        tilesToUpdate.Add(new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1));
        tilesToUpdate.Add(new Vector2Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY));
        tilesToUpdate.Add(new Vector2Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY));
        
        foreach (Vector2Int tilePos in tilesToUpdate)
        {
            UpdateDugGroundTile(tilePos.x, tilePos.y);
        }
    }

    private void UpdateDugGroundTile(int x, int y)
    {
        GridPropertyDetails gridProp = GetGridDetials(x, y);
        if (gridProp != null && gridProp.daysSinceDug > -1)
        {
            Tile dugTile = SetDugTile(x, y);
            if (dugTile != null)
            {
                groundDecoration1.SetTile(new Vector3Int(x, y, 0), dugTile);
            }
        }
    }

    private Tile SetDugTile(int xGrid, int yGrid)
    {
        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);

        #region 根据周围地块是否被挖掘来设置可以链接的地块
        if (dugGround == null || dugGround.Length == 0)
        {
            Debug.LogError("dugGround array is not initialized!");
            return null;
        }

        int tileIndex = -1;

        bool hasUp = upDug;
        bool hasDown = downDug;
        bool hasRight = rightDug;
        bool hasLeft = leftDug;

        if (!hasUp && !hasDown && !hasRight && !hasLeft)
        {
            tileIndex = 0;
        }
        else if (!hasUp && hasDown && hasRight && !hasLeft)
        {
            tileIndex = 1;
        }
        else if (!hasUp && hasDown && hasRight && hasLeft)
        {
            tileIndex = 2;
        }
        else if (!hasUp && hasDown && !hasRight && hasLeft)
        {
            tileIndex = 3;
        }
        else if (!hasUp && hasDown && !hasRight && !hasLeft)
        {
            tileIndex = 4;
        }
        else if (hasUp && hasDown && hasRight && !hasLeft)
        {
            tileIndex = 5;
        }
        else if (hasUp && hasDown && hasRight && hasLeft)
        {
            tileIndex = 6;
        }
        else if (hasUp && hasDown && !hasRight && hasLeft)
        {
            tileIndex = 7;
        }
        else if (hasUp && hasDown && !hasRight && !hasLeft)
        {
            tileIndex = 8;
        }
        else if (hasUp && !hasDown && hasRight && !hasLeft)
        {
            tileIndex = 9;
        }
        else if (hasUp && !hasDown && hasRight && hasLeft)
        {
            tileIndex = 10;
        }
        else if (hasUp && !hasDown && !hasRight && hasLeft)
        {
            tileIndex = 11;
        }
        else if (hasUp && !hasDown && !hasRight && !hasLeft)
        {
            tileIndex = 12;
        }
        else if (!hasUp && !hasDown && hasRight && !hasLeft)
        {
            tileIndex = 13;
        }
        else if (!hasUp && !hasDown && hasRight && hasLeft)
        {
            tileIndex = 14;
        }
        else if (!hasUp && !hasDown && !hasRight && hasLeft)
        {
            tileIndex = 15;
        }

        if (tileIndex >= 0 && tileIndex < dugGround.Length)
        {
            return dugGround[tileIndex];
        }
        else
        {
            Debug.LogWarning($"Tile index {tileIndex} is out of bounds for dugGround array (Length: {dugGround.Length}). Using default tile.");
            return dugGround[0];
        }
        #endregion
    }
    private bool IsGridSquareDug(int xGrid,int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridDetials(xGrid, yGrid);
        if(gridPropertyDetails == null)
        {
            return false;
        }
        else if(gridPropertyDetails.daysSinceDug>-1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void DisplayGridPropertyDetails()
    {
        foreach(KeyValuePair<string,GridPropertyDetails > item in gridPropertyDict)
        {
            GridPropertyDetails gridPropertyDetails = item.Value;
            DisplayDugGround(gridPropertyDetails);
        }
    }
    public void SetGridDetials(int x, int y, GridPropertyDetails gridPropertyDetails)
    {
        SetGridDetials(x, y, gridPropertyDetails, gridPropertyDict);
    }
    public void SetGridDetials(int x, int y, GridPropertyDetails gridPropertyDetails,Dictionary<string,GridPropertyDetails> gridPropertyDict)
    {
        string key = "x" + x + "y" + y;  

        gridPropertyDetails.gridX = x;
        gridPropertyDetails.gridY = y;

        gridPropertyDict[key] = gridPropertyDetails;
    }

    public GridPropertyDetails GetGridDetials(int x, int y, Dictionary<string, GridPropertyDetails> gridPropertyDict)
    {
        string key = "x" + x + "y" + y;
        GridPropertyDetails gridPropertyDetails;
        if (!gridPropertyDict.TryGetValue(key,out gridPropertyDetails))
        {
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }
    public GridPropertyDetails GetGridDetials(int x,int y)
    {
        if(gridPropertyDict == null)
        {
            InitialiseGridProperties();
        }
        
        if(gridPropertyDict == null)
        {
            return null;
        }
        
        return GetGridDetials(x, y, gridPropertyDict);
    }

    public Parsnip GetCropObjectAtGridPosition(int x, int y)
    {
        if (grid == null)
        {
            return null;
        }

        Vector3 cellCenterWorld = grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
        Collider2D[] colliders = Physics2D.OverlapPointAll(cellCenterWorld);

        foreach (Collider2D collider in colliders)
        {
            Parsnip crop = collider.gameObject.GetComponentInParent<Parsnip>();
            if (crop != null)
            {
                return crop;
            }

            crop = collider.gameObject.GetComponentInChildren<Parsnip>();
            if (crop != null)
            {
                return crop;
            }
        }

        return null;
    }
    private void AfterScemeLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();

        GameObject groundDeco1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1);
        if (groundDeco1 != null)
        {
            groundDecoration1 = groundDeco1.GetComponent<Tilemap>();
        }

        GameObject groundDeco2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2);
        if (groundDeco2 != null)
        {
            groundDecoration2 = groundDeco2.GetComponent<Tilemap>();
        }

        if (grid == null)
        {
            return;
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableStoreScene(string sceneName)
    {
        //存储当前场景网格信息
        gameObjectSave.sceneData.Remove(sceneName);
        SceneSave sceneSave = new SceneSave();
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDict;
        GameObjectSave.sceneData.Add(sceneName, sceneSave); 
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        //获取当前场景我们初始化的sceneSave物品
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDict = sceneSave.gridPropertyDetailsDictionary;
            }
            //如果存在耕地
            if (gridPropertyDict.Count > 0)
            {
                ClearDisplayGroundDetails();

                DisplayGridPropertyDetails();
            }
        }
    }

}
