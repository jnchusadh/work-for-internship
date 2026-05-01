using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemNudge : MonoBehaviour
{
    private WaitForSeconds pause;
    private bool isAnimating;
    private void Awake()
    {
        pause = new WaitForSeconds(0.04f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(isAnimating == false)
        {
            if (collision.transform.position.x < gameObject.transform.position.x)
            {
                StartCoroutine(RotateClocks());
            }
            else
            {
                StartCoroutine(RatateAntiClocks());
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
       if(isAnimating == false)
        {
            if (collision.transform.position.x < gameObject.transform.position.x)
            {
                StartCoroutine(RatateAntiClocks());
            }
            else
            {
                StartCoroutine(RotateClocks());
            }
        }
    }

    IEnumerator RatateAntiClocks()
    {
        isAnimating = true;
        for(int i = 0; i < 4; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);
            yield return pause;
        }
        for(int i = 0; i < 5; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);
            yield return pause;
        }
        gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);
        yield return pause;
        isAnimating = false;
    }

    IEnumerator RotateClocks()
    {
        isAnimating = true;
        for (int i = 0; i < 4; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);
            yield return pause;
        }
        for (int i = 0; i < 5; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);
            yield return pause;
        }
        gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);
        yield return pause;
        isAnimating = false;
    }
}
