using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeManager : Singleton<SceneChangeManager>
{
    public SceneName startSceneName;
    public bool autoLoadStartScene = true;
    private bool hasInitialized = false;


    private bool isFading;
    [SerializeField] private float fadingDuration = 1f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image fadingImage = null;

    public void FadeAndLoadScene(string sceneName,Vector3 spawnPos)
    {
        //Debug.Log(&"FadeAndLoadScene called: {sceneName}, isFading: {isFading}");
        if(isFading == false)
        {
            StartCoroutine(FadeAndchangeScene(sceneName.ToString(), spawnPos));
        }
    }
    IEnumerator Start()
    {
        if (hasInitialized)
        {
            yield break;
        }
        hasInitialized = true;

        fadingImage.color = new Color(0f, 0f, 0f, 1f);
        fadeCanvasGroup.alpha = 1f;

        
        /*
        if (autoLoadStartScene)
        {
            yield return StartCoroutine(LoadSceneAndSetActive(startSceneName.ToString()));
            EventHandler.CallAfterSceneLoadEvent();
            SaveLoadManager.Instance.StoreCurrentSceneData();
            yield return StartCoroutine(Fade(0f));
        }
        */
    }
    IEnumerator FadeAndchangeScene(string sceneName, Vector3 spawnPos)
    {
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        fadingImage.color = new Color(0f, 0f, 0f, 1f);  
        yield return StartCoroutine(Fade(1f));  // 淡出到不透明


        SaveLoadManager.Instance.StoreCurrentSceneData();
        PlayerManager.Instance.gameObject.transform.position = spawnPos;
        EventHandler.CallBeforeSceneUnloadEvent();

        yield return StartCoroutine(UnloadAllGameScenes());

        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        EventHandler.CallAfterSceneLoadEvent();

        SaveLoadManager.Instance.RestoreCurrentSceneData();
       yield return StartCoroutine(Fade(0f));  // 淡入到透明
        EventHandler.CallAfterSceneLoadFadeInEvent();
    }
    IEnumerator Fade(float finalAlpha)
    {
        isFading = true;
        //切换期间禁用玩家鼠标
        fadeCanvasGroup.blocksRaycasts = true;
        float fadeSpeed = Mathf.Abs(fadeCanvasGroup.alpha - finalAlpha) / fadingDuration;
        while (!Mathf.Approximately(fadeCanvasGroup.alpha, finalAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, finalAlpha,
                fadeSpeed * Time.deltaTime);
            yield return null;
        }
        isFading = false;

        //解除禁用
        fadeCanvasGroup.blocksRaycasts = false;
    }
    IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        //Debug.Log(&"开始加载场景：{sceneName}");
        
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        Scene newLoadScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newLoadScene);
        
        //Debug.Log(&"场景 {sceneName} 加载完成，当前激活场景：{newLoadScene.name}");
    }
    
    IEnumerator UnloadAllGameScenes()
    {
        List<string> scenesToUnload = new List<string>();
        
        // 收集需要卸载的场景
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            
            // 只卸载游戏世界场景（buildIndex >= 1）
            if (scene.buildIndex >= 1)
            {
                scenesToUnload.Add(scene.name);
            }
        }
        
        // 逐个卸载，每个都等待完成
        foreach (string sceneName in scenesToUnload)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
            
            // 等待卸载完成
            while (!unloadOp.isDone) 
            {
                yield return null;
            }
            
            yield return new WaitForEndOfFrame();  
        }
        
    }
}

