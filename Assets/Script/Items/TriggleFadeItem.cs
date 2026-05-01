using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggleFadeItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ItemsFade[] itemsFades = collision.gameObject.GetComponentsInChildren<ItemsFade>();
        if (itemsFades.Length > 0)
        {
            for(int i = 0; i < itemsFades.Length; i++)
            {
                itemsFades[i].FadeIn();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        ItemsFade[] itemsFades = collision.gameObject.GetComponentsInChildren<ItemsFade>();
        if (itemsFades.Length > 0)
        {
            for (int i = 0; i < itemsFades.Length; i++)
            {
                itemsFades[i].FadeOut();
            }
        }
    }
}
