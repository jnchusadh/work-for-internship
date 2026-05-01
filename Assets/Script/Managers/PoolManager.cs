using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, int> currentPoolSize = new Dictionary<string, int>();
    private Dictionary<string, int> maxPoolSizeDict = new Dictionary<string, int>();
    private Dictionary<GameObject, string> prefabToPoolKey = new Dictionary<GameObject, string>();
    
    [SerializeField] Pool[] pool = null;
    [SerializeField] private Transform objectPoolTransform = null;
    
    private void Start()
    {
        if (pool != null)
        {
            for(int i = 0; i < pool.Length; i++)
            {
                if (pool[i].prefab != null)
                {
                    CreatPool(pool[i].prefab, pool[i].poolSize, pool[i].maxPoolSize);
                }
                else
                {
                    Debug.LogWarning($"Pool[{i}].prefab is null, skipping");
                }
            }
        }
        else
        {
            Debug.LogWarning("Pool array is null, no pools created");
        }
    }
    
    private void CreatPool(GameObject prefab, int poolSize, int maxPoolSize = -1)
    {
        // 使用预制体的原始名称作为 Key（去掉可能存在的 (Clone) 后缀）
        string poolKey = GetOriginalPrefabName(prefab);
        string prefabName = poolKey;
        
        // 记录 prefab 到 poolKey 的映射
        if (!prefabToPoolKey.ContainsKey(prefab))
        {
            prefabToPoolKey.Add(prefab, poolKey);
        }
        
        // 创建父对象
        GameObject parentGameObject = new GameObject(prefabName + " Anchor");
        parentGameObject.transform.SetParent(objectPoolTransform);
        
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
            
            // 记录当前池大小和最大容量
            currentPoolSize.Add(poolKey, 0);
            if (maxPoolSize > 0)
            {
                maxPoolSizeDict.Add(poolKey, maxPoolSize);
            }
            else
            {
                maxPoolSizeDict.Add(poolKey, int.MaxValue); 
            }
            
            for(int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;
                newObject.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObject);
                currentPoolSize[poolKey]++;  // 更新当前大小
            }
        }
    }
    
    private string GetOriginalPrefabName(GameObject prefab)
    {
        string name = prefab.name;
        
        // 循环移除所有 (Clone) 后缀（可能有多个）
        while (name.Contains("(Clone)"))
        {
            int cloneIndex = name.IndexOf("(Clone)");
            if (cloneIndex >= 0)
            {
                name = name.Substring(0, cloneIndex).TrimEnd();
            }
        }
        
        return name;
    }
    
    public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string poolKey = GetPoolKeyForPrefab(prefab);
        
        if (poolDictionary.ContainsKey(poolKey))
        {
            if (poolDictionary[poolKey].Count > 0)
            {
                // 从对象池取出
                GameObject objectToReuse = GetObjectFromPool(poolKey);
                if (objectToReuse != null)
                {
                    ResetObject(position, rotation, objectToReuse, prefab, poolKey);
                    objectToReuse.SetActive(true);  
                    return objectToReuse;
                }
            }
            
            // 对象池为空，尝试扩容
            if (currentPoolSize[poolKey] < maxPoolSizeDict[poolKey])
            {
                Debug.LogWarning($"Pool empty for {prefab}, expanding pool...");
                return ExpandPoolAndGet(prefab, position, rotation, poolKey);
            }
            else
            {
                // 已达到最大容量，创建临时对象（不加入池）
                Debug.LogWarning($"Pool at max capacity for {prefab}, creating temporary object");
                GameObject tempObject = Instantiate(prefab, position, rotation);
                tempObject.transform.SetParent(objectPoolTransform);
                tempObject.SetActive(true);
                return tempObject;
            }
        }
        else
        {
            Debug.LogError($"No object pool for {prefab} (name={prefab.name}, poolKey={poolKey})");
            return null;
        }
    }
    
    private string GetPoolKeyForPrefab(GameObject prefab)
    {
        if (prefabToPoolKey.ContainsKey(prefab))
        {
            return prefabToPoolKey[prefab];
        }
        
        string poolKey = GetOriginalPrefabName(prefab);
        if (!prefabToPoolKey.ContainsKey(prefab))
        {
            prefabToPoolKey.Add(prefab, poolKey);
        }
        return poolKey;
    }
    
    // 扩容方法
    private GameObject ExpandPoolAndGet(GameObject prefab, Vector3 position, Quaternion rotation, string poolKey)
    {
        Transform parentTransform = null;
        if (poolDictionary[poolKey].Count > 0)
        {
            parentTransform = poolDictionary[poolKey].Peek().transform.parent;
        }
        else
        {
            // 如果队列为空，使用对象池的父对象
            parentTransform = objectPoolTransform;
        }
        
        GameObject newObject = Instantiate(prefab, parentTransform);
        newObject.SetActive(true);
        ResetObject(position, rotation, newObject, prefab, poolKey);
        currentPoolSize[poolKey]++;
        return newObject;
    }
    
    // 添加对象回收方法
    public void ReturnToPool(GameObject prefab, GameObject objectToReturn)
    {
        string poolKey = GetPoolKeyForPrefab(prefab);
        if (poolDictionary.ContainsKey(poolKey))
        {
            objectToReturn.SetActive(false); 
            poolDictionary[poolKey].Enqueue(objectToReturn);
        }
        else
        {
            Debug.Log($"No pool found for {prefab}, destroying object");
            Destroy(objectToReturn);
        }
    }
    
    private GameObject GetObjectFromPool(string poolKey)
    {
        if (poolDictionary[poolKey].Count > 0)
        {
            GameObject objectToReuse = poolDictionary[poolKey].Dequeue();
            return objectToReuse;
        }
        return null;
    }
    
    private void ResetObject(Vector3 position, Quaternion rotation, GameObject objectToReuse, GameObject prefab, string poolKey)
    {
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;
        objectToReuse.transform.localScale = prefab.transform.localScale;
        
        // 设置正确的父对象
        if (poolDictionary.ContainsKey(poolKey))
        {
            Transform parentTransform = poolDictionary[poolKey].Peek().transform.parent;
            objectToReuse.transform.SetParent(parentTransform);
        }
        
        // 重置 Rigidbody2D
        Rigidbody2D rb2D = objectToReuse.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
    }
}

[System.Serializable]
public struct Pool
{
    public int poolSize;
    public GameObject prefab;
    public int maxPoolSize;
}
