using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="so_ItemList",menuName ="Scriptable Object/Item/Item List")]
public class ScriptableObjectList : ScriptableObject
{
    [SerializeField]
    public List<ItemsDetails> itemsDetails;
}
