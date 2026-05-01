using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CameraBounds
{
    public Vector2 maxPosition;
    public Vector2 minPosition;
}

public class CameraFollow : MonoBehaviour
{
    public Transform playerPosition;
    public float smoothSpeed;
    
    [SerializeField] private CameraBounds farmBounds;
    [SerializeField] private CameraBounds fieldBounds;
    [SerializeField] private CameraBounds cabinBounds;
    
    private Vector2 currentMinPosition;
    private Vector2 currentMaxPosition;
    
    private void Start()
    {
        // 初始化默认边界
        if (farmBounds.minPosition == Vector2.zero && farmBounds.maxPosition == Vector2.zero)
        {
            farmBounds.minPosition = new Vector2(-50f, -50f);
            farmBounds.maxPosition = new Vector2(50f, 50f);
        }
        if (fieldBounds.minPosition == Vector2.zero && fieldBounds.maxPosition == Vector2.zero)
        {
            fieldBounds.minPosition = new Vector2(-100f, -100f);
            fieldBounds.maxPosition = new Vector2(100f, 100f);
        }
        if (cabinBounds.minPosition == Vector2.zero && cabinBounds.maxPosition == Vector2.zero)
        {
            cabinBounds.minPosition = new Vector2(-30f, -30f);
            cabinBounds.maxPosition = new Vector2(30f, 30f);
        }
        
        // 设置初始边界
        currentMinPosition = farmBounds.minPosition;
        currentMaxPosition = farmBounds.maxPosition;
    }
    
    private void Awake()
    {
        // 订阅场景加载事件
        EventHandler.AfterSceneLoadEvent += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // 取消订阅
        EventHandler.AfterSceneLoadEvent -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded()
    {
        Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
        if (activeScene.name.Contains("Farm"))
        {
            currentMinPosition = farmBounds.minPosition;
            currentMaxPosition = farmBounds.maxPosition;
        }
        else if (activeScene.name.Contains("Field"))
        {
            currentMinPosition = fieldBounds.minPosition;
            currentMaxPosition = fieldBounds.maxPosition;
        }
        else if (activeScene.name.Contains("Cabin"))
        {
            currentMinPosition = cabinBounds.minPosition;
            currentMaxPosition = cabinBounds.maxPosition;
        }
    }
    
    private void LateUpdate()
    {
        if (playerPosition != null)
        {
            Vector3 targetPos = playerPosition.position;
            
            // 限制玩家位置在边界内
            targetPos.x = Mathf.Clamp(targetPos.x, currentMinPosition.x, currentMaxPosition.x);
            targetPos.y = Mathf.Clamp(targetPos.y, currentMinPosition.y, currentMaxPosition.y);
            
            // 平滑移动摄像机
            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, targetPos.x, smoothSpeed * Time.deltaTime),
                Mathf.Lerp(transform.position.y, targetPos.y, smoothSpeed * Time.deltaTime),
                transform.position.z
            );
        }
    }
}
