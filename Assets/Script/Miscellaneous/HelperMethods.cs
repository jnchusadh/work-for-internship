using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods 
{
    public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentAtBoxPosition,Vector2 point, Vector2 size,float angle)
    {
        bool found = false;
        List<T> componentList = new List<T>();
        Collider2D[] collider2DArray = Physics2D.OverlapBoxAll(point, size, angle);
        for(int i = 0; i < collider2DArray.Length; i++)
        {
            T tcomponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tcomponent != null)
            {
                found = true;
                componentList.Add(tcomponent);
            }
            else
            {
                tcomponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tcomponent != null)
                {
                    found = true;
                    componentList.Add(tcomponent);
                }
            }
        }
        listComponentAtBoxPosition = componentList;
        return found;
    }
    //获取在指定位置T类型的组件。如果至少找到一个组件，则返回true并将找到的组件存放在componentAtPositionList列表中。
    public static bool GetComponentAtCursorLocation<T>(out List<T> componentsAtPositionList,Vector3 posToCheck)
    {
        bool found = false;
        List<T> componentList = new List<T>();
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(posToCheck);
        //遍历所有碰撞箱获取T类型的物品
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            T tcomponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tcomponent != null)
            {
                found = true;
                componentList.Add(tcomponent);
            }
            else
            {
                tcomponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tcomponent != null)
                {
                    found = true;
                    componentList.Add(tcomponent);
                }
            }
        }
        componentsAtPositionList = componentList;
        return found;
    }
    //检测一个矩形区域内的所有碰撞体将这些碰撞体转换成指定类型 T 的组件返回一个包含所有匹配组件的数组
    public static T[] GetComponentsAtBoxLocationNonAlloc<T>(int numberOfCollidersToTest,Vector2 point,Vector2 size,float angle)
    {
        Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];
        Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);
        T tcomponent = default(T);
        T[] componentArray = new T[collider2DArray.Length];
        for(int i = collider2DArray.Length - 1; i >= 0; i--)
        {
            if (collider2DArray[i] != null)
            {
                tcomponent = collider2DArray[i].gameObject.GetComponent<T>();
                if (tcomponent != null)
                {
                    componentArray[i] = tcomponent;
                }
            }
        }
        return componentArray;
    }
}
