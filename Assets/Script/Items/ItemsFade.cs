using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsFade : MonoBehaviour
{
    private SpriteRenderer sprite;
    void Awake()
    {
        sprite =gameObject.GetComponent<SpriteRenderer>();
    }

   public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }
    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        float currentAlpha = sprite.color.a;
        float distance = currentAlpha - Settings.targetAlpha;
        while (currentAlpha - Settings.targetAlpha > 0.01f)
        {
            currentAlpha = currentAlpha - distance / Settings.fadeOutTime * Time.deltaTime;
            sprite.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        sprite.color = new Color(1f, 1f, 1f, Settings.targetAlpha);
    }

    IEnumerator FadeOutRoutine()
    {
        float currentAlpha = sprite.color.a;
        float distance = 1f - Settings.targetAlpha;
        while (1f - currentAlpha > 0.01f)
        {
            currentAlpha = 1f - distance / Settings.fadeInTime * Time.deltaTime;
            sprite.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }
}
