using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(GenerateGUID))]
public class SceneItemManager : Singleton<SceneItemManager>,ISaveable
{
    private Transform parentTransform;
    private string iSaveableUniqueID;
    private GameObjectSave gameObjectSave;
    [SerializeField] private GameObject itemPrefab = null;

  

    

    public string ISaveableUniqueId
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
    protected override void Awake()
    {
        base.Awake();
        GameObjectSave = new GameObjectSave();
        //Debug.Log("SceneItemManager Awake - 调用注册");
        ISaveableRegister();
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        //Debug.Log("SceneItemManager OnEnable - 订阅事件");
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }
    private void AfterSceneLoad()
    {
        GameObject parentObj = GameObject.FindGameObjectWithTag(Tags.ItemParentTransform);
        if (parentObj != null)
        {
            parentTransform = parentObj.transform;
        }
        //Debug.Log("SceneItemManager AfterSceneLoad - parentTransform: " + parentTransform);
    }
    //场景切换时销毁场景中的所有物品
    private void DestorySceneItem()
    {
        Items[] itemsInScene = GameObject.FindObjectsOfType<Items>();
        for(int i = itemsInScene.Length - 1; i >= 0; i--)
        {
            Destroy(itemsInScene[i].gameObject);
        }
    }
    public void InstantiateSceneItem(int itemCode,Vector3 itemPos)
    {
        GameObject itemObject = Instantiate(itemPrefab, itemPos, Quaternion.identity,parentTransform);
        if (itemObject != null)
        {
            Items item = itemObject.GetComponent<Items>();
            item.Init(itemCode);
        }
    }
    private void InstantiateSceneItems(List<SceneItem> sceneItemList)
    {
        GameObject itemObject = null;

        //用于统计每个物品代码需要生成的数量
        Dictionary<int, int> itemCountDict = new Dictionary<int, int>();

        //统计每个物品应该生成多少个
        for(int i = 0; i < sceneItemList.Count; i++)
        {
            int itemCode = sceneItemList[i].itemCode;
            if(!itemCountDict.ContainsKey(itemCode))
            {
                itemCountDict[itemCode] = 0;
            }
            itemCountDict[itemCode]++;
        }

        //扣除玩家背包中已有的物品数量
        List<int> itemCodesToRemove = new List<int>();
        // 先复制所有 Key 到列表，避免遍历中修改 Dictionary
        List<int> keys = new List<int>(itemCountDict.Keys);
        foreach(int itemCode in keys)
        {
            int savedCount = itemCountDict[itemCode];

            //查找玩家背包中该物品的数量
            int playerItemPos = InventoryManager.Instance.FindItemInList(InventoryLocation.player, itemCode);
            int playerItemCount = 0;
            if(playerItemPos != -1)
            {
                playerItemCount = InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player][playerItemPos].itemQuanity;
            }

            //计算实际需要生成的数量
            int actualCount = savedCount - playerItemCount;
            if(actualCount <= 0)
            {
                //玩家已经拾取了所有该物品，不需要生成
                itemCodesToRemove.Add(itemCode);
                itemCountDict[itemCode] = 0;
            }
            else
            {
                itemCountDict[itemCode] = actualCount;
            }
        }

        //第三遍：实例化物品（根据调整后的数量）
        Dictionary<int, int> spawnedCount = new Dictionary<int, int>();
        for(int i = 0; i < sceneItemList.Count; i++)
        {
            int itemCode = sceneItemList[i].itemCode;

            if(itemCodesToRemove.Contains(itemCode))
            {
                continue;
            }

            if(!spawnedCount.ContainsKey(itemCode))
            {
                spawnedCount[itemCode] = 0;
            }

            //检查是否已经生成了足够的数量
            if(spawnedCount[itemCode] < itemCountDict[itemCode])
            {
                itemObject = Instantiate(itemPrefab, new Vector3(sceneItemList[i].position.x,
                    sceneItemList[i].position.y, sceneItemList[i].position.z), Quaternion.identity, parentTransform);
                if (itemObject != null)
                {
                    Items item = itemObject.GetComponent<Items>();
                    item.Items_code = sceneItemList[i].itemCode;
                    item.name = sceneItemList[i].itemName;
                    spawnedCount[itemCode]++;
                }
            }
        }
    }
    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        if(GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.listSceneItemList != null )
            {
                DestorySceneItem();
                InstantiateSceneItems(sceneSave.listSceneItemList);
            }
            else
            {
                Debug.LogWarning("没有找到 sceneItemList 数据");
            }
        }
        //else
        //{
        //    Debug.LogWarning("没有找到场景 " + sceneName + " 的保存数据!");
        //}
    }


    public void ISaveableStoreScene(string sceneName)
    {
        // 存储当前场景（不包括玩家手中的物品）
        GameObjectSave.sceneData.Remove(sceneName);


        List<SceneItem> sceneItemList = new List<SceneItem>();
        Items[] itemInScene = FindObjectsOfType<Items>();

        //遍历当前场景中的所有物品并添加到sceneItemList列表中
        for (int i = 0; i < itemInScene.Length; i++)
        {
            SceneItem sceneItem = new SceneItem();
            sceneItem.itemCode = itemInScene[i].Items_code;
            sceneItem.position = new Vector3Serializable(itemInScene[i].transform.position.x,
                itemInScene[i].transform.position.y, itemInScene[i].transform.position.z);
            sceneItem.itemName = itemInScene[i].name;
            sceneItemList.Add(sceneItem);
        }
        //将当前场景的物品列表添加到当前场景的物品列表中
        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneItemList = sceneItemList;
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    } 

}
